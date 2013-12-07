﻿using System;
using System.Collections.Generic;
using System.Text;
using Jint.Native;
using System.Reflection;
using System.Text.RegularExpressions;
using Jint.Native.Interop;
using PropertyAttributes = Jint.Native.PropertyAttributes;

namespace Jint
{
    /// <summary>
    /// Marshals clr objects to js objects and back. It can marshal types, delegates and other types of objects.
    /// </summary>
    /// <remarks>
    /// <pre>
    /// Marshaller holds a reference to a global object which is used to get a prototype while marshalling from
    /// clr to js. Futhermore a marshaller is to be accessible while running a script, therefore it strictly
    /// linked to the global object which defines a runtime environment for the script.
    /// </pre>
    /// </remarks>
    public class Marshaller
    {
        private readonly JintRuntime _runtime;
        private readonly Dictionary<Type, JsObject> _typeCache = new Dictionary<Type, JsObject>();
        private readonly Dictionary<Type, Delegate> _arrayMarshallers = new Dictionary<Type, Delegate>();
        private JsObject _typeType;

        public JsGlobal Global { get; private set; }

        // Assuming that Object supports IConvertable.

        private static readonly bool[,] IntegralTypeConversions =
        {
        //      Empty   Object  DBNull  Boolean Char    SByte   Byte    Int16   UInt16  Int32   UInt32  Int64   UInt64  Single  Double  Decimal DateTim -----   String
/*Empty*/   {   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true    },
/*Objec*/   {   false,  false,  false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  true    },
/*DBNul*/   {   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true    },
/*Boole*/   {   false,  false,  false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  false,  true    },
/*Char */   {   false,  false,  false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  false,  false,  false,  false,  true    },
/*SByte*/   {   false,  false,  false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  false,  true    },
/*Byte */   {   false,  false,  false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  false,  true    },
/*Int16*/   {   false,  false,  false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  false,  true    },
/*UInt1*/   {   false,  false,  false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  false,  true    },
/*Int32*/   {   false,  false,  false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  false,  true    },
/*UInt3*/   {   false,  false,  false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  false,  true    },
/*Int64*/   {   false,  false,  false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  false,  true    },
/*UInt6*/   {   false,  false,  false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  false,  true    },
/*Singl*/   {   false,  false,  false,  true,   false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  false,  true    },
/*Doubl*/   {   false,  false,  false,  true,   false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  false,  true    },
/*Decim*/   {   false,  false,  false,  true,   false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  false,  true    },
/*DateT*/   {   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  true    },
/*-----*/   {   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false   },
/*Strin*/   {   false,  false,  false,  true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   true,   false,  true    }
        };

        public Marshaller(JintRuntime runtime, JsGlobal global)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");
            if (global == null)
                throw new ArgumentNullException("global");

            _runtime = runtime;
            Global = global;
        }

        internal void Initialize()
        {
            // We can't initialize a _typeType property since _global.Marshaller should be initialized.
            _typeType = NativeFactory.BuildNativeConstructor(
                Global,
                typeof(Type),
                Global.FunctionClass.Prototype
            );

            _typeCache[typeof(Type)] = _typeType;

            // TODO: Replace a native constructors with appropriate JS constructors.

            foreach (var type in new[]
            {
                typeof(Int16),
                typeof(Int32),
                typeof(Int64),
                typeof(UInt16),
                typeof(UInt32),
                typeof(UInt64),
                typeof(Single),
                typeof(Double),
                typeof(Byte),
                typeof(SByte)
            })
            {
                _typeCache[type] = CreateConstructor(type, Global.NumberClass.Prototype);
            }

            _typeCache[typeof(String)] = CreateConstructor(typeof(String), Global.StringClass.Prototype);
            _typeCache[typeof(Char)] = CreateConstructor(typeof(Char), Global.StringClass.Prototype);
            _typeCache[typeof(Boolean)] = CreateConstructor(typeof(Boolean), Global.BooleanClass.Prototype);
            _typeCache[typeof(DateTime)] = CreateConstructor(typeof(DateTime), Global.DateClass.Prototype);
            _typeCache[typeof(Regex)] = CreateConstructor(typeof(Regex), Global.RegExpClass.Prototype);
        }

        /// <summary>
        /// Marshals a native value to a JsBox.
        /// </summary>
        public JsBox MarshalClrValue<T>(T value)
        {
            if (value == null)
                return JsBox.Null;

            if (value is JsBox)
                return (JsBox)(object)value;
            if (value is JsInstance)
                return JsBox.FromInstance((JsInstance)(object)value);

            if (value is Type)
            {
                var type = value as Type;
                if (type.IsGenericTypeDefinition)
                {
                    // Generic definitions aren't types in the meaning of JS
                    // but they are instances of System.Type.

                    return JsBox.CreateObject(Wrap(_typeType, type));
                }

                return JsBox.CreateObject(MarshalType(type));
            }

            return JsBox.CreateObject(Wrap(MarshalType(value.GetType()), value));
        }

        private JsObject Wrap(JsObject constructor, object value)
        {
            // We go through the real constructor because it needs to do some
            // setup for us. However, the constructor tries to instantiate the
            // object for us, but we don't want that. NativeFactory.WrappingMarker
            // disables this functionality.

            var @this = Global.CreateObject(NativeFactory.WrappingMarker, constructor.Prototype);

            constructor.Execute(_runtime, JsBox.CreateObject(@this), JsBox.EmptyArray, null);

            @this.Value = value;

            return @this;
        }

        public JsObject MarshalType(Type type)
        {
            JsObject result;
            if (!_typeCache.TryGetValue(type, out result))
            {
                result = CreateConstructor(type);
                _typeCache.Add(type, result);
            }

            return result;
        }

        private JsObject CreateConstructor(Type type)
        {
            return NativeFactory.BuildNativeConstructor(
                Global,
                type,
                _typeType.Prototype
            );
        }

        /// <summary>
        /// Creates a constructor for a native type and sets its 'prototype' property to
        /// the object derived from a rootPrototype.
        /// </summary>
        /// <remarks>
        /// For example native strings should be derived from <c>'String'</c> class i.e. they should
        /// contain a <c>String.prototype</c> object in theirs prototype chain.
        /// </remarks>
        private JsObject CreateConstructor(Type type, JsObject rootPrototype)
        {
            // BUG: Instead of the normal SetupNativeProperties, this
            // would call the SetupNativeProperties from _typeType and not from the
            // type that we were provided.

            return NativeFactory.BuildNativeConstructor(
                Global,
                type,
                rootPrototype
            );
        }

        /// <summary>
        /// Marshals a JsBox to a native value.
        /// </summary>
        /// <typeparam name="T">A native object type</typeparam>
        /// <param name="value">A JsBox to marshal</param>
        /// <returns>A converted native value</returns>
        public T MarshalJsValue<T>(JsBox value)
        {
            var valueInstance = value.ToInstance();
            object valueValue = valueInstance.Value;

            if (valueValue is T)
                return (T)valueValue;

            if (typeof(T).IsArray)
            {
                if (!value.IsValid || value.IsNullOrUndefined)
                    return default(T);

                if (
                    value.IsObject &&
                    Global.ArrayClass.HasInstance((JsObject)value)
                ) {
                    Delegate marshaller;
                    if (!_arrayMarshallers.TryGetValue(typeof(T), out marshaller))
                    {
                        _arrayMarshallers[typeof(T)] = marshaller = Delegate.CreateDelegate(
                            typeof(Func<JsObject, T>),
                            this,
                            typeof(Marshaller)
                                .GetMethod("MarshalJsFunctionHelper")
                                .MakeGenericMethod(typeof(T).GetElementType())
                        );
                    }

                    return ((Func<JsObject, T>)marshaller)((JsObject)value);
                }

                throw new JintException("Array is required");
            }

            if (typeof(Delegate).IsAssignableFrom(typeof(T)))
            {
                if (!value.IsValid || value.IsNullOrUndefined)
                    return default(T);

                if (!value.IsObject)
                    throw new JintException("Can't convert a non function object to a delegate type");

                return (T)(object)ProxyHelper.MarshalJsFunction(
                    _runtime,
                    (JsObject)value,
                    Global.PrototypeSink,
                    typeof(T)
                );
            }

            if (!value.IsNullOrUndefined)
            {
                if (value is T)
                    return (T)(object)value;
                if (valueInstance is T)
                    return (T)(object)valueInstance;
            }

            // JsNull and JsUndefined will fall here and become a nulls
            return (T)Convert.ChangeType(valueValue, typeof(T));
        }

        /// <summary>
        /// Gets a type of a native object represented by the current JsBox.
        /// If JsBox is a pure JsObject than returns a type of js object itself.
        /// </summary>
        /// <remarks>
        /// If a value is a wrapper around native value (like String, Number or a marshaled native value)
        /// this method returns a type of a stored value.
        /// If a value is an js object (constructed with a pure js function) this method returns
        /// a type of this value (for example JsArray, JsObject)
        /// </remarks>
        /// <param name="value">JsBox value</param>
        /// <returns>A Type object</returns>
        public Type GetInstanceType(JsBox value)
        {
            if (!value.IsValid || value.IsNullOrUndefined)
                return null;

            var instance = value.ToInstance();
            return (instance.Value ?? instance).GetType();
        }

        /// <summary>
        /// Marshals a native property to a descriptor
        /// </summary>
        /// <param name="property">Property to marshal</param>
        /// <param name="owner">Owner of the returned descriptor</param>
        /// <returns>A descriptor</returns>
        public NativeDescriptor MarshalPropertyInfo(PropertyInfo property, JsObject owner)
        {
            WrappedGetter getter;
            WrappedSetter setter = null;

            if (property.CanRead && property.GetGetMethod() != null)
                getter = ProxyHelper.WrapGetProperty(property);
            else
                getter = (global, @this) => JsBox.Undefined;

            if (property.CanWrite && property.GetSetMethod() != null)
                setter = ProxyHelper.WrapSetProperty(property);

            var attributes = PropertyAttributes.None;
            if (setter == null)
                attributes |= PropertyAttributes.ReadOnly;

            return new NativeDescriptor(owner, property.Name, getter, setter, attributes);
        }

        /// <summary>
        /// Marshals a native field to a JS Descriptor
        /// </summary>
        /// <param name="field">Field info to marshal</param>
        /// <param name="owner">Owner for the descriptor</param>
        /// <returns>Descriptor</returns>
        public NativeDescriptor MarshalFieldInfo(FieldInfo field, JsObject owner)
        {
            return new NativeDescriptor(
                owner,
                field.Name,
                ProxyHelper.WrapGetField(field),
                ProxyHelper.WrapSetField(field),
                PropertyAttributes.None
            );
        }

        public bool IsAssignable(Type target, Type source)
        {
            return
                (
                    typeof(IConvertible).IsAssignableFrom(source) &&
                    IntegralTypeConversions[(int)Type.GetTypeCode(source), (int)Type.GetTypeCode(target)]
                ) ||
                target.IsAssignableFrom(source);
        }

        public Type[] MarshalGenericArguments(JsBox[] genericArguments)
        {
            if (genericArguments == null)
                return Type.EmptyTypes;

            var result = new Type[genericArguments.Length];

            for (int i = 0; i < genericArguments.Length; i++)
            {
                result[i] = MarshalJsValue<Type>(genericArguments[i]);
            }

            return result;
        }
    }
}
