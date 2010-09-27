
if not exist docs mkdir docs

copy Retlang\bin\Release\Retlang.dll docs
copy Retlang\bin\Release\Retlang.xml docs\comments.xml
cd docs 

REM ********** Set path for .net framework2.0, sandcastle,hhc,hxcomp****************************
set PATH=%DXROOT%ProductionTools;C:\Program Files (x86)\HTML Help Workshop;C:\Program Files (x86)\Microsoft Help 2.0 SDK;%PATH%

if exist output rmdir output /s /q
if exist chm rmdir chm /s /q


REM ********** Call MRefBuilder ****************************

MRefBuilder Retlang.dll /out:reflection.org

REM ********** Apply Transforms ****************************

if {%1} == {vs2005} (
XslTransform /xsl:"%DXROOT%\ProductionTransforms\ApplyVSDocModel.xsl" reflection.org /xsl:"%DXROOT%\ProductionTransforms\AddFriendlyFilenames.xsl" /out:reflection.xml /arg:IncludeAllMembersTopic=true /arg:IncludeInheritedOverloadTopics=true
) else if {%1} == {hana} (
XslTransform /xsl:"%DXROOT%\ProductionTransforms\ApplyVSDocModel.xsl" reflection.org /xsl:"%DXROOT%\ProductionTransforms\AddFriendlyFilenames.xsl" /out:reflection.xml /arg:IncludeAllMembersTopic=false /arg:IncludeInheritedOverloadTopics=true
 ) else (
 XslTransform /xsl:"%DXROOT%\ProductionTransforms\ApplyPrototypeDocModel.xsl" reflection.org /xsl:"%DXROOT%\ProductionTransforms\AddGuidFilenames.xsl" /out:reflection.xml 
)

XslTransform /xsl:"%DXROOT%\ProductionTransforms\ReflectionToManifest.xsl"  reflection.xml /out:manifest.xml

call "%DXROOT%\Presentation\%1\copyOutput.bat"

REM ********** Call BuildAssembler ****************************
BuildAssembler /config:"%DXROOT%\Presentation\%1\configuration\sandcastle.config" manifest.xml

REM **************Generate an intermediate Toc file that simulates the Whidbey TOC format.

if {%1} == {prototype} (
XslTransform /xsl:"%DXROOT%\ProductionTransforms\createPrototypetoc.xsl" reflection.xml /out:toc.xml 
) else (
XslTransform /xsl:"%DXROOT%\ProductionTransforms\createvstoc.xsl" reflection.xml /out:toc.xml 
)

REM ************ Generate CHM help project ******************************

if not exist chm mkdir chm
if not exist chm\html mkdir chm\html
if not exist chm\icons mkdir chm\icons
if not exist chm\scripts mkdir chm\scripts
if not exist chm\styles mkdir chm\styles
if not exist chm\media mkdir chm\media

xcopy output\icons\* chm\icons\ /y /r
xcopy output\media\* chm\media\ /y /r
xcopy output\scripts\* chm\scripts\ /y /r
xcopy output\styles\* chm\styles\ /y /r

ChmBuilder.exe /project:Retlang.dll /html:Output\html /lcid:1033 /toc:Toc.xml /out:Chm

DBCSFix.exe /d:Chm /l:1033 

hhc chm\Retlang.dll.hhp


REM ************ Generate HxS help project **************************************

call "%DXROOT%\Presentation\shared\copyhavana.bat" Retlang.dll

XslTransform /xsl:"%DXROOT%\ProductionTransforms\CreateHxc.xsl" toc.xml /arg:fileNamePrefix=Retlang.dll /out:Output\Retlang.dll.HxC

XslTransform /xsl:"%DXROOT%\ProductionTransforms\TocToHxSContents.xsl" toc.xml /out:Output\Retlang.dll.HxT
