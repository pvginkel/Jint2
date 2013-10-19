using System;
using System.Collections.Generic;
using System.Text;
using Jint.Expressions;
using Antlr.Runtime;
using Jint.Native;
using Jint.Delegates;
using System.Security;
using System.Security.Permissions;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Jint.Parser;

namespace Jint {
    [Serializable]
    public class JintEngine {
        private ExecutionVisitor _visitor;

        [System.Diagnostics.DebuggerStepThrough]
        public JintEngine()
            : this(Options.Ecmascript5 | Options.Strict) {
        }

        [System.Diagnostics.DebuggerStepThrough]
        public JintEngine(Options options) {
            _visitor = new ExecutionVisitor(options);
            _permissionSet = new PermissionSet(PermissionState.None);
            _visitor.AllowClr = _allowClr;

            var global = _visitor.Global as JsObject;

            global["ToBoolean"] = _visitor.Global.FunctionClass.New(new Func<object, Boolean>(Convert.ToBoolean));
            global["ToByte"] = _visitor.Global.FunctionClass.New(new Func<object, Byte>(Convert.ToByte));
            global["ToChar"] = _visitor.Global.FunctionClass.New(new Func<object, Char>(Convert.ToChar));
            global["ToDateTime"] = _visitor.Global.FunctionClass.New(new Func<object, DateTime>(Convert.ToDateTime));
            global["ToDecimal"] = _visitor.Global.FunctionClass.New(new Func<object, Decimal>(Convert.ToDecimal));
            global["ToDouble"] = _visitor.Global.FunctionClass.New(new Func<object, Double>(Convert.ToDouble));
            global["ToInt16"] = _visitor.Global.FunctionClass.New(new Func<object, Int16>(Convert.ToInt16));
            global["ToInt32"] = _visitor.Global.FunctionClass.New(new Func<object, Int32>(Convert.ToInt32));
            global["ToInt64"] = _visitor.Global.FunctionClass.New(new Func<object, Int64>(Convert.ToInt64));
            global["ToSByte"] = _visitor.Global.FunctionClass.New(new Func<object, SByte>(Convert.ToSByte));
            global["ToSingle"] = _visitor.Global.FunctionClass.New(new Func<object, Single>(Convert.ToSingle));
            global["ToString"] = _visitor.Global.FunctionClass.New(new Func<object, String>(Convert.ToString));
            global["ToUInt16"] = _visitor.Global.FunctionClass.New(new Func<object, UInt16>(Convert.ToUInt16));
            global["ToUInt32"] = _visitor.Global.FunctionClass.New(new Func<object, UInt32>(Convert.ToUInt32));
            global["ToUInt64"] = _visitor.Global.FunctionClass.New(new Func<object, UInt64>(Convert.ToUInt64));
        }

        /// <summary>
        /// A global object associated with this engine instance
        /// </summary>
        public IGlobal Global {
            get { return _visitor.Global; }
        }

        private bool _allowClr;
        private PermissionSet _permissionSet;

        public static Program Compile(string source) {
            Program program = null;
            if (!string.IsNullOrEmpty(source)) {
                var lexer = new ES3Lexer(new ANTLRStringStream(source));
                var parser = new ES3Parser(new CommonTokenStream(lexer));

                program = parser.Execute();

                if (parser.Errors != null && parser.Errors.Count > 0) {
                    throw new JintException(String.Join(Environment.NewLine, parser.Errors.ToArray()));
                }
            }

            return program;
        }

        /// <summary>
        /// Pre-compiles the expression in order to check syntax errors.
        /// If errors are detected, the Error property contains the message.
        /// </summary>
        /// <returns>True if the expression syntax is correct, otherwiser False</returns>
        public static bool HasErrors(string script, out string errors) {
            try {
                errors = null;
                Program program = Compile(script);

                // In case HasErrors() is called multiple times for the same expression
                return program != null;
            }
            catch (Exception e) {
                errors = e.Message;
                return true;
            }
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <returns>Optionaly, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(string script) {
            return Run(script, true);
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="program">The expression tree to execute</param>
        /// <returns>Optionaly, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(Program program) {
            return Run(program, true);
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="reader">The TextReader to read script from</param>
        /// <returns>Optionaly, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(TextReader reader) {
            return Run(reader.ReadToEnd());
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="reader">The TextReader to read script from</param>
        /// <param name="unwrap">Whether to unwrap the returned value to a CLR instance. <value>True</value> by default.</param>
        /// <returns>Optionaly, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(TextReader reader, bool unwrap) {
            return Run(reader.ReadToEnd(), unwrap);
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <param name="unwrap">Whether to unwrap the returned value to a CLR instance. <value>True</value> by default.</param>
        /// <returns>Optionaly, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(string script, bool unwrap) {

            if (script == null)
                throw new
                    ArgumentException("Script can't be null", "script");

            Program program;



            try {
                program = Compile(script);
            }
            catch (Exception e) {
                throw new JintException("An unexpected error occured while parsing the script", e);
            }

            if (program == null)
                return null;

            return Run(program, unwrap);
        }

        /// <summary>
        /// Runs a set of JavaScript statements and optionally returns a value if return is called
        /// </summary>
        /// <param name="program">The expression tree to execute</param>
        /// <param name="unwrap">Whether to unwrap the returned value to a CLR instance. <value>True</value> by default.</param>
        /// <returns>Optionaly, returns a value from the scripts</returns>
        /// <exception cref="System.ArgumentException" />
        /// <exception cref="System.Security.SecurityException" />
        /// <exception cref="Jint.JintException" />
        public object Run(Program program, bool unwrap) {
            if (program == null)
                throw new
                    ArgumentException("Script can't be null", "script");

            _visitor.PermissionSet = _permissionSet;
            _visitor.AllowClr = _allowClr;
            _visitor.Result = null;

            try {
                _visitor.Visit(program);
            }
            catch (SecurityException) {
                throw;
            }
            catch (JsException e) {
                string message = e.Message;
                if (e.Value is JsError)
                    message = e.Value.Value.ToString();
                var stackTrace = new StringBuilder();
                var source = String.Empty;

                if (_visitor.CurrentStatement.Source != null) {
                    source = Environment.NewLine + _visitor.CurrentStatement.Source.ToString()
                            + Environment.NewLine + _visitor.CurrentStatement.Source.Code;
                }

                throw new JintException(message + source + stackTrace, e);
            }
            catch (Exception e) {
                StringBuilder stackTrace = new StringBuilder();
                string source = String.Empty;

                if (_visitor.CurrentStatement != null && _visitor.CurrentStatement.Source != null) {
                    source = Environment.NewLine + _visitor.CurrentStatement.Source.ToString()
                            + Environment.NewLine + _visitor.CurrentStatement.Source.Code;
                }

                throw new JintException(e.Message + source + stackTrace, e);
            }

            return _visitor.Result == null ? null : unwrap ? _visitor.Global.Marshaller.MarshalJsValue<object>( _visitor.Result) : _visitor.Result;
        }

        #region SetParameter overloads

        /// <summary>
        /// Defines an external object to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the object duting the execution of the script</param>
        /// <param name="value">Available object</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, object value) {
            _visitor.GlobalScope[name] = _visitor.Global.WrapClr(value);
            return this;
        }

        /// <summary>
        /// Defines an external Double value to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the Double value during the execution of the script</param>
        /// <param name="value">Available Double value</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, double value) {
            _visitor.GlobalScope[name] = _visitor.Global.NumberClass.New(value);
            return this;
        }

        /// <summary>
        /// Defines an external String instance to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the String instance during the execution of the script</param>
        /// <param name="value">Available String instance</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, string value) {
            if (value == null)
                _visitor.GlobalScope[name] = JsNull.Instance;
            else
                _visitor.GlobalScope[name] = _visitor.Global.StringClass.New(value);
            return this;
        }

        /// <summary>
        /// Defines an external Int32 value to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the Int32 value during the execution of the script</param>
        /// <param name="value">Available Int32 value</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, int value) {
            _visitor.GlobalScope[name] = _visitor.Global.WrapClr(value);
            return this;
        }

        /// <summary>
        /// Defines an external Boolean value to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the Boolean value during the execution of the script</param>
        /// <param name="value">Available Boolean value</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, bool value) {
            _visitor.GlobalScope[name] = _visitor.Global.BooleanClass.New(value);
            return this;
        }

        /// <summary>
        /// Defines an external DateTime value to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the DateTime value during the execution of the script</param>
        /// <param name="value">Available DateTime value</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, DateTime value) {
            _visitor.GlobalScope[name] = _visitor.Global.DateClass.New(value);
            return this;
        }
        #endregion

        public JintEngine AddPermission(IPermission perm) {
            _permissionSet.AddPermission(perm);
            return this;
        }

        public JintEngine SetFunction(string name, JsFunction function) {
            _visitor.GlobalScope[name] = function;
            return this;
        }

        public object CallFunction(string name, params object[] args) {
            JsInstance oldResult = _visitor.Result;
            _visitor.Visit(new Identifier(name));
            var returnValue = CallFunction((JsFunction)_visitor.Result, args);
            _visitor.Result = oldResult;
            return returnValue;
        }

        public object CallFunction(JsFunction function, params object[] args) {
            _visitor.ExecuteFunction(function, null, Array.ConvertAll<object,JsInstance>( args, x => _visitor.Global.Marshaller.MarshalClrValue<object>(x) ));
            return _visitor.Global.Marshaller.MarshalJsValue<object>(_visitor.Returned);
        }

        public JintEngine SetFunction(string name, Delegate function) {
            _visitor.GlobalScope[name] = _visitor.Global.FunctionClass.New(function);
            return this;
        }

        /// <summary>
        /// Escapes a JavaScript string literal
        /// </summary>
        /// <param name="value">The string literal to espace</param>
        /// <returns>The escaped string literal, without sinlge quotes, back slashes and line breaks</returns>
        public static string EscapteStringLiteral(string value) {
            return value.Replace("\\", "\\\\").Replace("'", "\\'").Replace(Environment.NewLine, "\\r\\n");
        }

        public JintEngine DisableSecurity() {
            _permissionSet = new PermissionSet(PermissionState.Unrestricted);
            return this;
        }

        public JintEngine AllowClr()
        {
            _allowClr = true;
            return this;
        }

        public JintEngine AllowClr(bool value)
        {
            _allowClr = value;
            return this;
        }

        public JintEngine EnableSecurity()
        {
            _permissionSet = new PermissionSet(PermissionState.None);
            return this;
        }

        public void Save(Stream s) {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(s, _visitor);
        }

        public static void Load(JintEngine engine, Stream s) {
            BinaryFormatter formatter = new BinaryFormatter();
            var visitor = (ExecutionVisitor)formatter.Deserialize(s);
            engine._visitor = visitor;
        }

        public static JintEngine Load(Stream s) {
            JintEngine engine = new JintEngine();
            Load(engine, s);
            return engine;
        }
    }
}
