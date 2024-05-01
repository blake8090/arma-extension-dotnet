writeLog = {
    diag_log format ["[ArmaExtensionDotNet] LOG: %1", _this];
};

execSqf = {
    _this spawn {
        "ArmaExtensionDotNet" callExtension ["sendResponse", [call compile _this]];
    };
};

addMissionEventHandler [
    "ExtensionCallback",
    {
        params ["_name", "_function", "_data"];

        diag_log format["ExtensionCallback - name: '%1', function: '%2', data: '%3'", _name, _function, _data];

        if (_name isEqualTo "ArmaExtensionDotNet") then {
            _func = missionNamespace getVariable [_function, objNull];
            
            if (_func isEqualTo objNull) then {
                hint "Function does not exist!";
            } else {
                _data call _func;
            };
        };
    }
];

_result = "ArmaExtensionDotNet" callExtension "runSqfTest";
systemChat _result;
