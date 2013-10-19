using System;
namespace Jint.Native
{
    public interface INativeIndexer
    {
        JsInstance Get(JsInstance that, JsInstance index);
        void Set(JsInstance that, JsInstance index, JsInstance value);
    }
}
