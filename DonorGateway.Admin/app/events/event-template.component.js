//event-template.component.js
(function () {
    var module = angular.module('app');

    function controller($scope, $http, log) {
        var $ctrl = this;

        $ctrl.title = 'Event Template';
        $ctrl.description = "Update Event Template";

        $ctrl.$onChanges = function () {
            $ctrl.refresh();
        }

        $ctrl.$onInit = function () {
            console.log('event template init');
        }

        $ctrl.refresh = function () {
            if ($ctrl.templateId === undefined) return;
            $ctrl.isBusy = true;
            $http.get('api/template/' + $ctrl.templateId).then(function (r) {
                $ctrl.template = r.data;
                $ctrl.templateDelta = angular.copy($ctrl.template); 
            }).catch(function (err) {
                console.log("Oops. Can't get event template", err);
                toastr.error("Oops. Can't get event template");
            }).finally(function () {
                $ctrl.isBusy = false;
            });
        }

        $ctrl.cancel = function() {
            $ctrl.template = angular.copy($ctrl.templateDelta); 
        }

        $ctrl.save = function () {
            $http.put('api/template', $ctrl.template).then(function (r) {
                angular.extend($ctrl.template, r.data);
                $ctrl.templateDelta = angular.copy($ctrl.template);
                log.success('Updated template');
            }).catch(function (err) {
                console.log('Oops. Error updating template', err);
                log.error('Oops. Something went wrong updating template: ' + err.data.message);
            });
        }

        $ctrl.fileSelected = function ($files, $file) {

            $ctrl.isBusy = true;
            var file = $file;

            var src = '';
            var reader = new FileReader();

            reader.onloadstart = function () { }

            reader.onload = function (e) {
                src = reader.result;
                $ctrl.template.image = reader.result;
                $ctrl.template.mimeType = file.type;
                $ctrl.isBusy = false;
            }

            reader.onerror = function (e) {
                console.log('reader error', e);
                log.error('Oops. Error reading image'); 
            }

            reader.onloadend = function (e) {
                //Added due to large images not complete before digest cycle. 
                $scope.$apply();
            };

            reader.readAsDataURL(file);
        };
    }

    module.component('eventTemplate',
        {
            bindings: {
                templateId: '<'
            },
            templateUrl: 'app/events/event-template.component.html',
            controller: ['$scope', '$http', 'toastr', controller]
        });
}
)();