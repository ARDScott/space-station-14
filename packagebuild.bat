@echo off
set zip="C:\Program Files\7-Zip\7z.exe"
set hour=%time:~0,2%
if "%time:~0,1%"==" " set hour=0%time:~1,1%
set dt=%date:~10,4%-%date:~4,2%-%date:~7,2%_%hour%%time:~3,2%
set dir=ss3d-%dt%
mkdir %dir%
mkdir %dir%\bin
mkdir %dir%\bin\client
mkdir %dir%\bin\client\client
mkdir %dir%\bin\server
mkdir %dir%\Media
copy Media\media.gorPack %dir%\Media
copy Media\Interface1.gui %dir%\Media
copy Media\Interface2.gui %dir%\Media
copy Media\Interface3.gui %dir%\Media
copy Buildfiles\* %dir%

xcopy /E SS3D_Client\bin\Release\* %dir%\bin\client\client
xcopy /E SS3d_server\bin\Release\* %dir%\bin\server
cd %dir%
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S *.svn*') DO RMDIR /S /Q "%%G"
cd ..
%zip% -tzip a %dir%.zip %dir%
rd /S /Q %dir%

pause