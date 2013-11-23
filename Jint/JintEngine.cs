using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Jint
{
    [Serializable]
    public class JintEngine
    {
        private IJintBackend _backend;

        [DebuggerStepThrough]
        public JintEngine()
            : this(Options.EcmaScript5 | Options.Strict)
        {
        }

        [DebuggerStepThrough]
        public JintEngine(Options options)
        {
            _backend = new Backend.Dlr.DlrBackend(options, this);
        }

        /// <summary>
        /// A global object associated with this engine instance
        /// </summary>
        public JsGlobal Global
        {
            get { return _backend.Global; }
        }

        public static ProgramSyntax Compile(string source)
        {
            ProgramSyntax program = null;
            if (!string.IsNullOrEmpty(source))
            {
                var lexer = new ES3Lexer(new ANTLRStringStream(source));
                var parser = new ES3Parser(new CommonTokenStream(lexer));

                program = parser.Execute();

                if (parser.Errors != null && parser.Errors.Count > 0)
                    throw new JintException(String.Join(Environment.NewLine, parser.Errors.ToArray()));
            }

            return program;
        }

        internal static BlockSyntax CompileBlockStatements(string source)
        {
            BlockSyntax block = null;
            if (!string.IsNullOrEmpty(source))
            {
                var lexer = new ES3Lexer(new ANTLRStringStream(source));
                var parser = new ES3Parser(new CommonTokenStream(lexer));

                block = parser.ExecuteBlockStatements();

                if (parser.Errors != null && parser.Errors.Count > 0)
                    throw new JintException(String.Join(Environment.NewLine, parser.Errors.ToArray()));
            }

            return block;
        }

        /// <summary>
        /// Pre-compiles the expression in order to check syntax errors.
        /// If errors are detected, the Error property contains the message.
        /// </summary>
        /// <returns>True if the expression syntax is correct, otherwiser False</returns>
        public static bool HasErrors(string script, out string errors)
        {
            try
            {
                errors = null;
                ProgramSyntax program = Compile(script);

                // In case HasErrors() is called multiple times for the same expression
                return program != null;
            }
            catch (Exception e)
            {
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
        public object Run(string script)
        {
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
        public object Run(ProgramSyntax program)
        {
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
        public object Run(TextReader reader)
        {
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
        public object Run(TextReader reader, bool unwrap)
        {
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
        public object Run(string script, bool unwrap)
        {
            if (script == null)
                throw new
                    ArgumentException("Script can't be null", "script");

            ProgramSyntax program;

            try
            {
                program = Compile(script);
            }
            catch (Exception e)
            {
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
        public object Run(ProgramSyntax program, bool unwrap)
        {
            try
            {
                return _backend.Run(program, unwrap);
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (JsException e)
            {
                string message = e.Message;

                if (e.Value is JsError)
                    message = e.Value.Value.ToString();

                throw new JintException(message, e);
            }
            catch (Exception e)
            {
                throw new JintException(e.Message, e);
            }
        }

        #region SetParameter overloads

        /// <summary>
        /// Defines an external object to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the object duting the execution of the script</param>
        /// <param name="value">Available object</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, object value)
        {
            _backend.Global[name] = _backend.Global.WrapClr(value);
            return this;
        }

        /// <summary>
        /// Defines an external Double value to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the Double value during the execution of the script</param>
        /// <param name="value">Available Double value</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, double value)
        {
            _backend.Global[name] = _backend.Global.NumberClass.New(value);
            return this;
        }

        /// <summary>
        /// Defines an external String instance to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the String instance during the execution of the script</param>
        /// <param name="value">Available String instance</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, string value)
        {
            if (value == null)
                _backend.Global[name] = JsNull.Instance;
            else
                _backend.Global[name] = _backend.Global.StringClass.New(value);
            return this;
        }

        /// <summary>
        /// Defines an external Int32 value to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the Int32 value during the execution of the script</param>
        /// <param name="value">Available Int32 value</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, int value)
        {
            _backend.Global[name] = _backend.Global.WrapClr(value);
            return this;
        }

        /// <summary>
        /// Defines an external Boolean value to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the Boolean value during the execution of the script</param>
        /// <param name="value">Available Boolean value</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, bool value)
        {
            _backend.Global[name] = _backend.Global.BooleanClass.New(value);
            return this;
        }

        /// <summary>
        /// Defines an external DateTime value to be available inside the script
        /// </summary>
        /// <param name="name">Local name of the DateTime value during the execution of the script</param>
        /// <param name="value">Available DateTime value</param>
        /// <returns>The current JintEngine instance</returns>
        public JintEngine SetParameter(string name, DateTime value)
        {
            _backend.Global[name] = _backend.Global.DateClass.New(value);
            return this;
        }
        #endregion

        public JintEngine AddPermission(IPermission perm)
        {
            _backend.PermissionSet.AddPermission(perm);
            return this;
        }

        public JintEngine SetFunction(string name, JsFunction function)
        {
            _backend.Global[name] = function;
            return this;
        }

        public object CallFunction(string name, params object[] args)
        {
            return _backend.CallFunction(name, args);
        }

        public object CallFunction(JsFunction function, params object[] args)
        {
            return _backend.CallFunction(function, args);
        }

        public JintEngine SetFunction(string name, Delegate function)
        {
            _backend.Global[name] = _backend.Global.FunctionClass.New(function);
            return this;
        }

        /// <summary>
        /// Escapes a JavaScript string literal
        /// </summary>
        /// <param name="value">The string literal to espace</param>
        /// <returns>The escaped string literal, without sinlge quotes, back slashes and line breaks</returns>
        public static string EscapteStringLiteral(string value)
        {
            return value.Replace("\\", "\\\\").Replace("'", "\\'").Replace(Environment.NewLine, "\\r\\n");
        }

        public JintEngine DisableSecurity()
        {
            _backend.PermissionSet = new PermissionSet(PermissionState.Unrestricted);
            return this;
        }

        public JintEngine AllowClr()
        {
            _backend.AllowClr = true;
            return this;
        }

        public JintEngine AllowClr(bool value)
        {
            _backend.AllowClr = value;
            return this;
        }

        public JintEngine EnableSecurity()
        {
            _backend.PermissionSet = new PermissionSet(PermissionState.None);
            return this;
        }

        public void Save(Stream s)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(s, _backend);
        }

        public static void Load(JintEngine engine, Stream s)
        {
            engine._backend = (IJintBackend)new BinaryFormatter().Deserialize(s);
        }

        public static JintEngine Load(Stream s)
        {
            JintEngine engine = new JintEngine();
            Load(engine, s);
            return engine;
        }
    }
}
