//event-detail.component.js
(function () {
    var module = angular.module('app');

    function controller($http, log) {
        var $ctrl = this;

        $ctrl.dateFormat = "MM/DD/YYYY h:mm a";
        $ctrl.hostLocation = window.__env.rsvpUrl + '/';

        $ctrl.$onChanges = function () {
            $ctrl.refresh();
        }

        $ctrl.$onInit = function () { console.log('event detail init');}

        $ctrl.refresh = function () {
            if ($ctrl.eventId === undefined) return;
            $ctrl.isBusy = true;
            $http.get('api/event/' + $ctrl.eventId).then(function (r) {
                $ctrl.event = r.data;
            }).catch(function (err) {
                console.log('Opps. Something went wrong', err);
            }).finally(function () {
                $ctrl.isBusy = false;
            });
        }

        $ctrl.delete = function () {
            $http.delete('api/event/' + $ctrl.eventId).then(function (r) {
                $ctrl.event = null;
                log.warning('Deleted event');
                $ctrl.onDelete();
            }).catch(function (err) {
                console.log('Oops. Something went wrong deleting event', err);
                log.error('Oops. Something went wrong deleting event');
            });
            
        }

        $ctrl.save = function () {
            return $http.put('api/event', $ctrl.event)
                .then(function (r) {
                    angular.extend($ctrl.event, r.data);
                    log.success('Updated ' + $ctrl.event.name);
                }).catch(function (err) {
                    console.log('Oops. Something went wrong saving event');
                    log.error('Oops. Something went wrong saving event');
                });
        }

        $ctrl.toggleCancel = function() {
            $ctrl.event.isCancelled = !$ctrl.event.isCancelled;
            $ctrl.save(); 
        }

    }

    module.component('eventDetail',
        {
            bindings: {
                eventId: '<',
                onDelete: '&'
            },
            templateUrl: 'app/events/event-detail.component.html',
            controller: ['$http', 'toastr', controller]
        });
}
)();