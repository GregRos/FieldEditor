using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PostSharp;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Reflection;

namespace FieldEditor.CustomAspects
{
	/// <summary>
	///   <para> An aspect that implements a property using a resource in the declaring instance. </para>
	///   <para> Note that this attribute can only be applied to types deriving from FrameworkElement. </para>
	/// </summary>
	[Serializable]
	[MulticastAttributeUsage(MulticastTargets.Property, TargetMemberAttributes = MulticastAttributes.Instance, AllowMultiple = false)]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly | AttributeTargets.Property)]
	public class ResourceBoundPropertyAspect : LocationInterceptionAspect
	{
		/// <summary>
		///   Optionally, specify an explicit resource key for the resource being represented. Equals the name of the target member by default.
		/// </summary>
		public string ResourceKey { set; private get; }

		public override void CompileTimeInitialize(LocationInfo targetLocation, AspectInfo aspectInfo)
		{
			if (ResourceKey == null)
				ResourceKey = targetLocation.Name;
		}

		public override bool CompileTimeValidate(LocationInfo locationInfo)
		{
			if (!typeof (FrameworkElement).IsAssignableFrom(locationInfo.DeclaringType))
			{
				Message.Write(locationInfo, SeverityType.Error, "Error2",
				              @"The type {0} must derive from FrameworkElement in order to decorate property '{1}' with ResourceBoundPropertyAspect.",
				              locationInfo.DeclaringType, locationInfo.PropertyInfo);

				return false;
			}
			return base.CompileTimeValidate(locationInfo);
		}

		public override void OnGetValue(LocationInterceptionArgs args)
		{
			var resources = (args.Instance as FrameworkElement).Resources;
			if (!resources.Contains(ResourceKey))
				throw new KeyNotFoundException("The key '{0}' was not found in the resource dictionary.".FormatWith(ResourceKey));
			args.Value = resources[ResourceKey];
		}

		public override void OnSetValue(LocationInterceptionArgs args)
		{
			var resources = (args.Instance as FrameworkElement).Resources;
			if (!resources.Contains(ResourceKey))
				throw new KeyNotFoundException("The key '{0}' was not found in the resource dictionary.".FormatWith(ResourceKey));
			resources[ResourceKey] = args.Value;
		}
	}
}