using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Jint.Bound;
using Jint.Expressions;

namespace Jint.Support
{
    internal class ILBuilder
    {
        private static readonly FieldInfo _stringEmpty = typeof(string).GetField("Empty");

        private readonly ILGenerator _il;
        private readonly ISymbolDocumentWriter _document;

        public ILBuilder(ILGenerator il, ISymbolDocumentWriter document)
        {
            if (il == null)
                throw new ArgumentNullException("il");

            _il = il;
            _document = document;
        }

        public void Emit(OpCode opcode)
        {
            _il.Emit(opcode);
        }

        public void Emit(OpCode opcode, byte arg)
        {
            _il.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, sbyte arg)
        {
            _il.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, short arg)
        {
            _il.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, int arg)
        {
            _il.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, MethodInfo meth)
        {
            _il.Emit(opcode, meth);
        }

        public void Emit(OpCode opcode, SignatureHelper signature)
        {
            _il.Emit(opcode, signature);
        }

        public void Emit(OpCode opcode, ConstructorInfo con)
        {
            _il.Emit(opcode, con);
        }

        public void Emit(OpCode opcode, Type cls)
        {
            _il.Emit(opcode, cls);
        }

        public void Emit(OpCode opcode, long arg)
        {
            _il.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, float arg)
        {
            _il.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, double arg)
        {
            _il.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, Label label)
        {
            _il.Emit(opcode, label);
        }

        public void Emit(OpCode opcode, Label[] labels)
        {
            _il.Emit(opcode, labels);
        }

        public void Emit(OpCode opcode, FieldInfo field)
        {
            _il.Emit(opcode, field);
        }

        public void Emit(OpCode opcode, string str)
        {
            _il.Emit(opcode, str);
        }

        public void Emit(OpCode opcode, LocalBuilder local)
        {
            _il.Emit(opcode, local);
        }

        public Label BeginExceptionBlock()
        {
            return _il.BeginExceptionBlock();
        }

        public void EndExceptionBlock()
        {
            _il.EndExceptionBlock();
        }

        public void BeginExceptFilterBlock()
        {
            _il.BeginExceptFilterBlock();
        }

        public void BeginCatchBlock(Type exceptionType)
        {
            _il.BeginCatchBlock(exceptionType);
        }

        public void BeginFaultBlock()
        {
            _il.BeginFaultBlock();
        }

        public void BeginFinallyBlock()
        {
            _il.BeginFinallyBlock();
        }

        public Label DefineLabel()
        {
            return _il.DefineLabel();
        }

        public void MarkLabel(Label loc)
        {
            _il.MarkLabel(loc);
        }

        public LocalBuilder DeclareLocal(Type localType)
        {
            return _il.DeclareLocal(localType);
        }

        public LocalBuilder DeclareLocal(Type localType, bool pinned)
        {
            return _il.DeclareLocal(localType, pinned);
        }

        public void MarkSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn)
        {
            _il.MarkSequencePoint(document, startLine, startColumn, endLine, endColumn);
        }

        internal BoundValueType EmitConstant(object value)
        {
            var stringValue = value as string;

            if (stringValue != null)
            {
                if (stringValue.Length == 0)
                    Emit(OpCodes.Ldsfld, _stringEmpty);
                else
                    Emit(OpCodes.Ldstr, stringValue);

                return BoundValueType.String;
            }
            
            if (value is double)
            {
                // It's a bit strange that we don't use the double overload
                // of Emit here, but the reason for this is that VM corruption
                // occurred with this overload, specifically with the following
                // sequence:
                //
                //   Emit(OpCodes.Ldc_R8, 1d);
                //   Emit(OpCodes.Box, typeof(double));
                //   Emit(OpCodes.Ret);
                //
                // Changing the 1d to BitConverter.DoubleToInt64Bits(1d),
                // switching to the long overload, fixed this without altering
                // the resulting program.

                Emit(OpCodes.Ldc_R8, BitConverter.DoubleToInt64Bits((double)value));

                return BoundValueType.Number;
            }
            
            if (value is bool)
            {
                Emit((bool)value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);

                return BoundValueType.Boolean;
            }

            if (value is int)
            {
                OpCode opCode;
                int intValue = (int)value;

                switch (intValue)
                {
                    case -1: opCode = OpCodes.Ldc_I4_M1; break;
                    case 0: opCode = OpCodes.Ldc_I4_0; break;
                    case 1: opCode = OpCodes.Ldc_I4_1; break;
                    case 2: opCode = OpCodes.Ldc_I4_2; break;
                    case 3: opCode = OpCodes.Ldc_I4_3; break;
                    case 4: opCode = OpCodes.Ldc_I4_4; break;
                    case 5: opCode = OpCodes.Ldc_I4_5; break;
                    case 6: opCode = OpCodes.Ldc_I4_6; break;
                    case 7: opCode = OpCodes.Ldc_I4_7; break;
                    case 8: opCode = OpCodes.Ldc_I4_8; break;
                    default:
                        if (intValue >= Byte.MinValue && intValue <= Byte.MaxValue)
                            Emit(OpCodes.Ldc_I4_S, (byte)intValue);
                        else
                            Emit(OpCodes.Ldc_I4, intValue);
                        return BoundValueType.Unset;
                }

                Emit(opCode);

                return BoundValueType.Unset;
            }

            throw new NotImplementedException();
        }

        public void EmitCall(MethodInfo methodInfo, BoundValueType returnType)
        {
            var actualReturnType = EmitCall(methodInfo);

            if (actualReturnType == returnType)
                return;

            if (returnType == BoundValueType.Unset)
                Emit(OpCodes.Pop);

            throw new InvalidOperationException();

            /*
            else if (returnType == BoundValueType.Unknown)
            {
                if (actualReturnType.IsValueType())
                    EmitBox(returnType);
                else
                    Emit(OpCodes.Castclass, typeof(object));
            }
            else if (actualReturnType == BoundValueType.Unknown)
            {
                if (returnType.IsValueType())
                    Emit(OpCodes.Unbox_Any, returnType.GetNativeType());
                else
                    Emit(OpCodes.Castclass, returnType.GetNativeType());
            }
            else
            {
                throw new NotImplementedException();
            }
             * */
        }

        public BoundValueType EmitCall(MethodInfo methodInfo)
        {
            Emit(
                methodInfo.IsStatic ? OpCodes.Call : OpCodes.Callvirt,
                methodInfo
            );

            return methodInfo.ReturnType.ToValueType();
        }

        public void EmitNot()
        {
            EmitConstant(0);
            Emit(OpCodes.Ceq);
        }

        public LocalBuilder EmitArray<T>(IEnumerable<T> items)
        {
            var local = DeclareLocal(typeof(T[]));

            var list = items.ToList();

            EmitConstant(list.Count);
            Emit(OpCodes.Newarr, typeof(T));

            Emit(OpCodes.Stloc, local);

            OpCode opCode;
            bool specifyIndex = true;

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Byte: opCode = OpCodes.Stelem_I1; break;
                case TypeCode.Int16: opCode = OpCodes.Stelem_I2; break;
                case TypeCode.Int32: opCode = OpCodes.Stelem_I4; break;
                case TypeCode.Int64: opCode = OpCodes.Stelem_I8; break;
                case TypeCode.Single: opCode = OpCodes.Stelem_R4; break;
                case TypeCode.Double: opCode = OpCodes.Stelem_R8; break;
                default:
                    specifyIndex = false;
                    if (typeof(T).IsValueType)
                        opCode = OpCodes.Stelem;
                    else
                        opCode = OpCodes.Stelem_Ref;
                    break;
            }

            for (int i = 0; i < list.Count; i++)
            {
                Emit(OpCodes.Ldloc, local);
                EmitConstant(i);

                if (!specifyIndex)
                {
                    EmitConstant(list[i]);

                    if (opCode == OpCodes.Stelem)
                        Emit(opCode, typeof(T));
                    else
                        Emit(opCode);
                }
                else
                {
                    Emit(opCode, i);
                }
            }

            return local;
        }

        public NamedLabel DefineLabel(string name)
        {
            return new NamedLabel(name, DefineLabel());
        }

        public void MarkLabel(NamedLabel label)
        {
            if (label == null)
                throw new ArgumentNullException("label");

            MarkLabel(label.Label);
        }

        public void MarkSequencePoint(SourceLocation location)
        {
            if (location == null)
                throw new ArgumentNullException("location");
            if (_document == null || location == SourceLocation.Missing)
                return;

            MarkSequencePoint(
                _document,
                location.StartLine,
                location.StartColumn + 1,
                location.EndLine,
                location.EndColumn + 1
            );
        }
    }
}
