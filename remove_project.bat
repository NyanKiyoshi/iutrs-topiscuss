@ECHO OFF
SET PROGRAM=%~nx0
SET PROJECT_TESTS="Tests"

:: Check parameters: only one is accepted.
::   1. Retrieve the project name;
::   2. First argument (PROJECT_NAME) must not be empty;
::   3. Second argument must be empty.
:parse
    SET project_name=%1
    IF "%project_name%"=="" GOTO usage
    IF NOT "%2"=="" GOTO usage

:dereference-project
    :: Dereference the probjet from the tests
    CD "%PROJECT_TESTS%" || GOTO failed
    DOTNET remove reference "..\%project_name%\%project_name%.csproj" || GOTO failed
    CD ..                || GOTO failed

:remove-project
    :: Add the project project to the root project
    DOTNET sln remove "%project_name%" || GOTO failed
    ECHO Note: to delete your project, run: @RD /S /Q "%project_name%"

:clean-exit
    EXIT /B 0

:failed
    ECHO %PROGRAM% has failed.
    EXIT /B 1

:usage
    ECHO %PROGRAM%--Dereference and remove a given project from the current solution.
    ECHO Usage: %PROGRAM% ^<PROJECT_NAME^> [PROJECT_TYPE=console]
    ECHO.
    ECHO PROJECT_NAME: the name of the project to delete.
    EXIT /B 1
