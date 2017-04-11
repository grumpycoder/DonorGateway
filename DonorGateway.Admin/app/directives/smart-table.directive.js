//mark.lawrence
//smart-table.clearKey.js

(function () {
    angular.module('app').directive("stClearKey", function () {
        return {
            restrict: 'EA',
            require: ['^stTable', '^ngModel'],
            link: function (scope, element, attrs, ctrls) {
                element.on('keydown', function (event) {
                    var ngModel = ctrls[1];
                    var ctrl = ctrls[0];

                    if (event.which === 13) {
                        return ctrl.pipe();
                    }
                    if (event.which === 27) {

                        if (element.context.attributes['st-search'] !== undefined) {
                            return scope.$apply(function () {
                                var fieldName = element.context.attributes['st-search'].value;
                                var tableState = ctrl.tableState();
                                tableState.search.predicateObject[fieldName] = '';
                                return ctrl.pipe();
                            });

                        } else {
                            return scope.$apply(function () {
                                ngModel.$setViewValue(null);
                                ngModel.$render(); // will update the input value as well
                            });

                        }
                    }
                });
            }
        };
    });

    angular.module('app')
        .directive("stResetSearch",
            function () {
                return {
                    restrict: 'EA',
                    require: ['^stTable', '^ngModel'],
                    link: function (scope, element, attrs, ctrls) {
                        return element.bind('click',
                            function () {
                                var model = ctrls[1];
                                var ctrl = ctrls[0];

                                return scope.$apply(function () {
                                    angular.forEach(model.$viewValue,
                                        function (value, key) {
                                            if (key.toLowerCase() === 'page') {
                                                model.$viewValue[key] = 1;
                                            }
                                            if (Array.isArray(value) || _.includes(key.toLowerCase(), 'page')) return;
                                            model.$viewValue[key] = null;
                                        });
                                    var tableState = ctrl.tableState();
                                    tableState.search.predicateObject = {};
                                    tableState.pagination.start = 0;
                                    return ctrl.pipe();
                                });
                            });
                    }
                };
            });

    angular.module('app').directive('stSubmitSearch', ['stConfig', '$timeout', '$parse', function (stConfig, $timeout, $parse) {
        return {
            require: '^stTable',
            link: function (scope, element, attr, ctrl) {
                return element.bind('click',
                    function () {
                        var tableCtrl = ctrl;
                        tableCtrl.pipe();
                    });

            }
        };
    }]);


    angular.module('smart-table')
      .directive('stSearch', ['stConfig', '$timeout', '$parse', function (stConfig, $timeout, $parse) {
          return {
              require: '^stTable',
              link: function (scope, element, attr, ctrl) {
                  var tableCtrl = ctrl;
                  var promise = null;
                  var throttle = attr.stDelay || stConfig.search.delay;
                  var event = attr.stInputEvent || stConfig.search.inputEvent;

                  attr.$observe('stSearch', function (newValue, oldValue) {
                      var input = element[0].value;
                      if (newValue !== oldValue && input) {
                          ctrl.tableState().search = {};
                          tableCtrl.search(input, newValue);
                      }
                  });

                  //table state -> view
                  scope.$watch(function () {
                      return ctrl.tableState().search;
                  }, function (newValue, oldValue) {
                      var predicateExpression = attr.stSearch || '$';
                      if (newValue.predicateObject && $parse(predicateExpression)(newValue.predicateObject) !== element[0].value) {
                          element[0].value = $parse(predicateExpression)(newValue.predicateObject) || '';
                      }
                  }, true);

                  // view -> table state
                  element.bind(event, function (evt) {
                      evt = evt.originalEvent || evt;
                      if (promise !== null) {
                          $timeout.cancel(promise);
                      }

                      promise = $timeout(function () {
                          tableCtrl.search(evt.target.value, attr.stSearch || '');
                          promise = null;
                      }, throttle);
                  });
              }
          };
      }]);

})();