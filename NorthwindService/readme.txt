Instructions to run WCF service:

	Run cmd.exe as administrator.
	
	%comspec% /k ""C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"" x86

	Change directory to "....NorthwindService\bin\debug\"

	wcfsvchost.exe /service:"Northwind.dll" /config:"app.config"


	