@echo off
ilmerge /internalize /keyfile:..\Jint\Key.snk /out:Jint.dll ..\Jint\bin\Release\Jint.dll ..\Dependencies\Antlr3.Runtime.dll
