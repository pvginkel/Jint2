using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Jint.Native;

namespace Jint.Runtime
{
    internal class JintContext
    {
        private readonly IGlobal _global;
        private readonly Dictionary<string, JintGetMemberBinder> _getMember = new Dictionary<string, JintGetMemberBinder>();
        private readonly Dictionary<string, JintSetMemberBinder> _setMember = new Dictionary<string, JintSetMemberBinder>();
        private readonly Dictionary<ExpressionType, JintUnaryOperationBinder> _unaryOperations = new Dictionary<ExpressionType, JintUnaryOperationBinder>();
        private readonly Dictionary<ExpressionType, JintBinaryOperationBinder> _binaryOperations = new Dictionary<ExpressionType, JintBinaryOperationBinder>();
        private readonly Dictionary<Type, JintConvertBinder> _explicitConverters = new Dictionary<Type, JintConvertBinder>();
        private readonly Dictionary<Type, JintConvertBinder> _implicitConverters = new Dictionary<Type, JintConvertBinder>();
        private readonly Dictionary<CallInfo, JintGetIndexBinder> _getIndex = new Dictionary<CallInfo, JintGetIndexBinder>();
        private readonly Dictionary<CallInfo, JintSetIndexBinder> _setIndex = new Dictionary<CallInfo, JintSetIndexBinder>();
        private readonly Dictionary<string, JintDeleteMemberBinder> _deleteMember = new Dictionary<string, JintDeleteMemberBinder>();
        private readonly Dictionary<CallInfo, JintDeleteIndexBinder> _deleteIndex = new Dictionary<CallInfo, JintDeleteIndexBinder>();

        public JintContext(IGlobal global)
        {
            if (global == null)
                throw new ArgumentNullException("global");

            _global = global;
        }

        public CallSiteBinder GetMember(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            JintGetMemberBinder result;
            if (!_getMember.TryGetValue(name, out result))
            {
                result = new JintGetMemberBinder(name);
                _getMember.Add(name, result);
            }

            return result;
        }

        public CallSiteBinder SetMember(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            JintSetMemberBinder result;
            if (!_setMember.TryGetValue(name, out result))
            {
                result = new JintSetMemberBinder(name);
                _setMember.Add(name, result);
            }

            return result;
        }

        public CallSiteBinder GetIndex(CallInfo callInfo)
        {
            if (callInfo == null)
                throw new ArgumentNullException("callInfo");

            JintGetIndexBinder result;
            if (!_getIndex.TryGetValue(callInfo, out result))
            {
                result = new JintGetIndexBinder(this, callInfo);
                _getIndex.Add(callInfo, result);
            }

            return result;
        }

        public CallSiteBinder SetIndex(CallInfo callInfo)
        {
            if (callInfo == null)
                throw new ArgumentNullException("callInfo");

            JintSetIndexBinder result;
            if (!_setIndex.TryGetValue(callInfo, out result))
            {
                result = new JintSetIndexBinder(callInfo);
                _setIndex.Add(callInfo, result);
            }

            return result;
        }

        public CallSiteBinder UnaryOperation(ExpressionType expressionType)
        {
            JintUnaryOperationBinder result;
            if (!_unaryOperations.TryGetValue(expressionType, out result))
            {
                result = new JintUnaryOperationBinder(this, expressionType);
                _unaryOperations.Add(expressionType, result);
            }

            return result;
        }

        public CallSiteBinder BinaryOperation(ExpressionType expressionType)
        {
            JintBinaryOperationBinder result;
            if (!_binaryOperations.TryGetValue(expressionType, out result))
            {
                result = new JintBinaryOperationBinder(_global, this, expressionType);
                _binaryOperations.Add(expressionType, result);
            }

            return result;
        }

        public CallSiteBinder Convert(Type type, bool @explicit)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var binders = @explicit ? _explicitConverters : _implicitConverters;

            JintConvertBinder result;
            if (!binders.TryGetValue(type, out result))
            {
                result = new JintConvertBinder(_global, type, @explicit);
                binders.Add(type, result);
            }

            return result;
        }

        public CallSiteBinder DeleteMember(string name)
        {
            JintDeleteMemberBinder result;
            if (!_deleteMember.TryGetValue(name, out result))
            {
                result = new JintDeleteMemberBinder(name);
                _deleteMember.Add(name, result);
            }

            return result;
        }

        public CallSiteBinder DeleteIndex(CallInfo callInfo)
        {
            JintDeleteIndexBinder result;
            if (!_deleteIndex.TryGetValue(callInfo, out result))
            {
                result = new JintDeleteIndexBinder(callInfo);
                _deleteIndex.Add(callInfo, result);
            }

            return result;
        }
    }
}
