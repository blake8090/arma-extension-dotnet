myFunction = {
	_text = format ["Extension Result: %1", _this];
	diag_log _text;
};

addMissionEventHandler [
	"ExtensionCallback",
	{
		params ["_name", "_function", "_data"];

		diag_log format["ExtensionCallback - name: '%1', function: '%2', data: '%3'", _name, _function, _data];

		if (_name isEqualTo "ArmaExtensionDotNet") then {
			diag_log "[ArmaExtensionDotNet] ExtensionCallback called";
			
			_func = missionNamespace getVariable [_function, objNull];
			if (_func isEqualTo objNull) then {
				hint "Function does not exist!";
			} else {
				_data call _func;
			};
		};
	}
];

// One argument example call
_result = "ArmaExtensionDotNet" callExtension "test";
hint _result;

// Multiple args example call
// _result = "ArmaExtensionDotNet" callExtension ["testFunction", ["abc", "123"]];
//hint format ["%1", _result];
