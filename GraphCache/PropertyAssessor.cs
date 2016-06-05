using System;
using System.Reflection;

namespace GraphCache
{
    internal delegate TProperty Getter<TOwner, TProperty>(TOwner owner);
    internal delegate void Setter<TOwner, TProperty>(TOwner owner, TProperty value);

    internal class PropertyAssessor<TOwner, TProperty> : PropertyAssessor
    {
        private Lazy<Getter<TOwner, TProperty>> _getter;
        private Lazy<Setter<TOwner, TProperty>> _setter;

        public PropertyAssessor(
            PropertyInfo propertyInfo,
            Lazy<Getter<TOwner, TProperty>> getter,
            Lazy<Setter<TOwner, TProperty>> setter)
            : base(propertyInfo)
        {
            _getter = getter;
            _setter = setter;
        }

        public override object GetValue(object owner) => _getter.Value((TOwner)owner);

        public override void SetValue(object owner, object value) => _setter.Value((TOwner)owner, (TProperty)value);
    }

    internal abstract class PropertyAssessor
    {
        public PropertyInfo PropertyInfo { get; private set; }

        public PropertyAssessor(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
        }

        public abstract object GetValue(object owner);
        public abstract void SetValue(object owner, object value);
    }
}
