writeLog = {
    diag_log format ["[ArmaExtensionDotNet] LOG: %1", _this];
};

execSqf = {
    _this spawn {
        private _a = parseSimpleArray _this;
        private _id = _a select 0;
        private _code = _a select 1;
        private _result = call compile _code;
        diag_log format["result: %1", _result];
        "ArmaExtensionDotNet" callExtension ["sendResponse", [_id, _result]];
    };
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

systemChat ("ArmaExtensionDotNet" callExtension "runSqfTest");
