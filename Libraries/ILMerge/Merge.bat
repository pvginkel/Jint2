@echo off
ilmerge /internalize /keyfile:..\..\Jint\Key.snk /out:Jint.dll ..\..\Jint\bin\Release\Jint.dll ..\Antlr3\Antlr3.Runtime.dll
