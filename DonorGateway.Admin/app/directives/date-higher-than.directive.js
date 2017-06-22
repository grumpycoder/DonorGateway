//mark.lawrence
//date-higher-than.directive.js

(function () {

    angular.module('app').directive('dateHigherThan',
        function ($window) {
            return {
                require: '^ngModel',
                restrict: 'A',
                link: function (scope, elm, attrs, ctrl) {
                    var moment = $window.moment;
                    var t, f
                    var validate = function (viewValue) {
                        var comparisonModel = attrs.dateHigherThan;
                        comparisonModel = comparisonModel.replace(/["']/g, "") + 'Z';

                        if (!viewValue || !comparisonModel) {
                            ctrl.$setValidity('dateHigherThan', true);
                            return viewValue;
                        }

                        ctrl.$setValidity('dateHigherThan', moment(comparisonModel).isBefore(moment(viewValue)));
                        return viewValue;
                    };

                    ctrl.$parsers.unshift(validate);
                    ctrl.$formatters.push(validate);

                    attrs.$observe('dateHigherThan', function (comparisonModel) {
                        // Whenever the comparison model changes we'll re-validate
                        return validate(ctrl.$viewValue);
                    });

                }
            };
        }
    );

})();

