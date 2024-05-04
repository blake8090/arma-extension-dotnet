writeLog = {
    diag_log format ["%1", _this];
};

execSqf = {
    private _args = _this splitString "|";
    private _id = _args select 0;
    private _code = _args select 1;
    diag_log format ["String <%1> - Request id: <%2> Code: <%3>", _this, _id, _code];
    private _result = call compile _code;
    "ArmaExtensionDotNet" callExtension ["sendResponse", [_id, _result]];
};

addMissionEventHandler [
    "ExtensionCallback",
    {
        params ["_name", "_function", "_data"];

        //diag_log format["ExtensionCallback - name: '%1', function: '%2', data: '%3'", _name, _function, _data];

        if (_name isEqualTo "ArmaExtensionDotNet" && _function isEqualTo "writeLog") exitWith {
            _data call writeLog;
        };

        if (_name isEqualTo "ArmaExtensionDotNet" && _function isEqualTo "execSqf") exitWith {
            _data call execSqf;
        };

        hint format["Function %1 does not exist!", _function];
    }
];

//systemChat ("ArmaExtensionDotNet" callExtension "runSqfTest");
