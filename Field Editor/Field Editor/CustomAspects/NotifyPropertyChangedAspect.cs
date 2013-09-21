using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PostSharp;
using Fasterflect;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Reflection;

namespace FieldEditor.CustomAspects
{
	/// <summary>
	///   Aspect for automatically implementing property change notification.
	/// </summary>
	[Serializable]
	[MulticastAttributeUsage(MulticastTargets.Property, TargetMemberAttributes = MulticastAttributes.Instance, AllowMultiple = false)]
	[AttributeUsage(
		AttributeTargets.Class
		| AttributeTargets.Property
		| AttributeTargets.Struct
		| AttributeTargets.Assembly,
		AllowMultiple = false)]
	public class NotifyPropertyChangedAspect : LocationInterceptionAspect, IInstanceScopedAspect
	{
		private const string EventName = "PropertyChanged";
		private MemberGetter _eventGetter;
		private string _propertyName;

		/// <summary>
		///   Constructs a new instance of this aspect.
		/// </summary>
		/// <param name="dependsOn">
		///   <para> An optional list of property names on which the value of this property depends. </para>
		///   <para> The aspect listens for the PropertyChanged event for these properties. </para>
		/// </param>
		public NotifyPropertyChangedAspect(params string[] dependsOn)
		{
			ThisDependsOn = dependsOn;
		}

		public string[] ThisDependsOn { set; private get; }

		public override void CompileTimeInitialize(LocationInfo targetLocation, AspectInfo aspectInfo)
		{
			_propertyName = targetLocation.Name;
			base.CompileTimeInitialize(targetLocation, aspectInfo);
		}

		public override bool CompileTimeValidate(LocationInfo locationInfo)
		{
			if (!typeof (INotifyPropertyChanged).IsAssignableFrom(locationInfo.DeclaringType))
			{
				Message.Write(locationInfo, SeverityType.Error, "Error1",
				              "The type '{0}' must implement INotifyPropertyChanged in order to decorate property '{1}' with NotifyPropertyChangedAspect.",
				              locationInfo.DeclaringType, locationInfo.PropertyInfo);
				return false;
			}
			return base.CompileTimeValidate(locationInfo);
		}

		public object CreateInstance(AdviceArgs adviceArgs)
		{
			var clone = MemberwiseClone() as NotifyPropertyChangedAspect;
			adviceArgs.Instance.AddHandler(EventName, arr =>
			                                          {
				                                          clone.Handler_PropertyChanged(arr[0], (PropertyChangedEventArgs) arr[1]);
				                                          return null;
			                                          });
			return clone;
		}

		private void Handler_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (ThisDependsOn.Contains(e.PropertyName))
				OnPropertyChanged(sender, _propertyName);
		}

		private void OnPropertyChanged(object instance, string propertyName)
		{
			if (_eventGetter(instance) != null) instance.InvokeDelegate(EventName, instance, new PropertyChangedEventArgs(propertyName));
		}

		public override void OnSetValue(LocationInterceptionArgs args)
		{
			args.ProceedSetValue();
			if (_eventGetter(args.Instance) == null) return;
			OnPropertyChanged(args.Instance, _propertyName);
		}

		public override void RuntimeInitialize(LocationInfo locationInfo)
		{
			_eventGetter = locationInfo.DeclaringType.DelegateForGetFieldValue(EventName);
			base.RuntimeInitialize(locationInfo);
		}

		public void RuntimeInitializeInstance()
		{
		}
	}
}