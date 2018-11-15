@ECHO OFF
SET PROGRAM=%~nx0
SET PROJECT_TESTS="Tests"

:: Check parameters: only two are accepted.
::   1. Retrieve the project name;
::   2. First argument (PROJECT_NAME) must not be empty;
::   3. Second argument (PROJECT_TYPE) can be empty, and will default to 'console'.
::   3. Third argument (invalid) must be empty.
:parse
    SET project_name=%1
    SET project_type=%2
    IF "%project_name%"=="" GOTO usage
    IF "%project_type%"=="" SET project_type=console
    IF NOT "%3"=="" GOTO usage

:create-project
    :: Create the class library project
    MKDIR "%project_name%"       || GOTO failed
    CD    "%project_name%"       || GOTO failed
    DOTNET new "%project_type%"  || GOTO failed
    CD ..                        || GOTO failed

    :: Add the project project to the root project
    DOTNET sln add "%project_name%" || GOTO failed

:create-tests
    :: Add project reference to the unit tests project
    CD    "%PROJECT_TESTS%" || GOTO failed
    DOTNET add reference "..\%project_name%" || GOTO failed

    :: Go back to the SLN root project
    CD ..  || GOTO failed

:clean-exit
    EXIT /B 0

:failed
    ECHO %PROGRAM% has failed.
    EXIT /B 1

:usage
    ECHO Usage: %PROGRAM% ^<PROJECT_NAME^> [PROJECT_TYPE=console]
    ECHO.
    ECHO PROJECT_NAME: the name of the project to create.
    ECHO.
    ECHO PROJECT_TYPE:
    ECHO    Templates                                         Short Name         Language          Tags
    ECHO    ----------------------------------------------------------------------------------------------------------------------------
    ECHO    Console Application                               console            [C#], F#, VB      Common/Console
    ECHO    Class library                                     classlib           [C#], F#, VB      Common/Library
    ECHO    Unit Test Project                                 mstest             [C#], F#, VB      Test/MSTest
    ECHO    NUnit 3 Test Project                              nunit              [C#], F#, VB      Test/NUnit
    ECHO    NUnit 3 Test Item                                 nunit-test         [C#], F#, VB      Test/NUnit
    ECHO    xUnit Test Project                                xunit              [C#], F#, VB      Test/xUnit
    ECHO    Razor Page                                        page               [C#]              Web/ASP.NET
    ECHO    MVC ViewImports                                   viewimports        [C#]              Web/ASP.NET
    ECHO    MVC ViewStart                                     viewstart          [C#]              Web/ASP.NET
    ECHO    ASP.NET Core Empty                                web                [C#], F#          Web/Empty
    ECHO    ASP.NET Core Web App (Model-View-Controller)      mvc                [C#], F#          Web/MVC
    ECHO    ASP.NET Core Web App                              razor              [C#]              Web/MVC/Razor Pages
    ECHO    ASP.NET Core with Angular                         angular            [C#]              Web/MVC/SPA
    ECHO    ASP.NET Core with React.js                        react              [C#]              Web/MVC/SPA
    ECHO    ASP.NET Core with React.js and Redux              reactredux         [C#]              Web/MVC/SPA
    ECHO    Razor Class Library                               razorclasslib      [C#]              Web/Razor/Library/Razor Class Library
    ECHO    ASP.NET Core Web API                              webapi             [C#], F#          Web/WebAPI
    ECHO    global.json file                                  globaljson                           Config
    ECHO    NuGet Config                                      nugetconfig                          Config
    ECHO    Web Config                                        webconfig                            Config
    ECHO    Solution File                                     sln                                  Solution

    EXIT /B 1
