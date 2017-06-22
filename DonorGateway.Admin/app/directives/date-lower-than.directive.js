//mark.lawrence
//date-lower-than.directive.js

(function () {

    angular.module('app').directive('dateLowerThan',
        function ($window) {
            return {
                require: '^ngModel',
                restrict: 'A',
                link: function (scope, elm, attrs, ctrl) {
                    var moment = $window.moment;
                    var t, f
                    var validate = function (viewValue) {
                        var comparisonModel = attrs.dateLowerThan;
                        comparisonModel = comparisonModel.replace(/["']/g, "") + 'Z';

                        if (!viewValue || !comparisonModel) {
                            ctrl.$setValidity('dateLowerThan', true);
                            return viewValue;
                        }

                        ctrl.$setValidity('dateLowerThan', moment(comparisonModel).isAfter(moment(viewValue)));
                        return viewValue;
                    };

                    ctrl.$parsers.unshift(validate);
                    ctrl.$formatters.push(validate);

                    attrs.$observe('dateLowerThan', function (comparisonModel) {
                        // Whenever the comparison model changes we'll re-validate
                        return validate(ctrl.$viewValue);
                    });

                }
            };
        }
    );

})();

