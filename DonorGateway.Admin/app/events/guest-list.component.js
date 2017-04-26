//guest-list.component.js
(function () {
    var module = angular.module('app');

    function controller($http, toastr) {
        var $ctrl = this;
        var tableStateRef;
        var pageSizeDefault = 10;
        var choices = [
            { id: 1, name: "Register", command: function (e) { $ctrl.registerGuest(e) }, icon: 'icon ion-key', default: true },
            { id: 2, name: "Mail Ticket", command: function (e) { $ctrl.sendMail(e) }, icon: 'icon ion-android-mail', default: false },
            { id: 3, name: "Cancel", command: function (e) { $ctrl.cancelRegistration(e) }, icon: 'icon ion-android-cancel', default: false },
            { id: 4, name: "Guest List", command: function (e) { $ctrl.reservationOverride(e) }, icon: 'icon ion-android-add-circle', default: false },
            { id: 5, name: "Add Tickets", command: function (e) { $ctrl.addGuestTicket(e) }, icon: 'icon ion-android-add-circle', default: false }
        ];


        $ctrl.title = 'Reservation Manager';
        $ctrl.description = "Manage Guest List";

        $ctrl.searchModel = {
            page: 1,
            pageSize: pageSizeDefault,
            orderBy: 'id',
            orderDirection: 'asc'
        };

        $ctrl.$onChanges = function () {
            $ctrl.search();
        }

        $ctrl.$onInit = function () {
            console.log('guest list init');
        }

        $ctrl.resetSearch = function () {
            $ctrl.searchModel = {
                page: 1,
                pageSize: pageSizeDefault,
                orderBy: 'id',
                orderDirection: 'asc'
            };
            $ctrl.quickFilter = null;
            $ctrl.search(tableStateRef);
        }

        $ctrl.search = function () {
            $ctrl.isBusy = true;
            if ($ctrl.eventId === undefined) return;
            $http.get('api/event/' + $ctrl.eventId + '/guests', { params: $ctrl.searchModel }).then(function (r) {
                $ctrl.searchModel = r.data;
                $ctrl.guests = [];
                r.data.results.map(function (guest) {
                    guest.choices = buildGuestOptions(guest);
                    $ctrl.guests.push(guest);
                });
                delete $ctrl.searchModel.results;
            }).catch(function (err) {
                console.log('Oops. Something went wrong', err);
            }).finally(function () {
                $ctrl.isBusy = false;
            });
        }

        $ctrl.paged = function paged() {
            $ctrl.search(tableStateRef);
        };

        $ctrl.quickFilterChange = function () {
            $ctrl.searchModel.page = 1;
            $ctrl.searchModel.isWaiting = null;
            $ctrl.searchModel.isMailed = null;
            $ctrl.searchModel.isAttending = null;
            $ctrl.searchModel.isMailed = null;

            switch ($ctrl.quickFilter) {
                case 'WaitingAndNotSent':
                    $ctrl.searchModel.isWaiting = true;
                    $ctrl.searchModel.isMailed = false;
                    break;
                case 'WaitingAndSent':
                    $ctrl.searchModel.isWaiting = true;
                    $ctrl.searchModel.isMailed = true;;
                    break;
                case 'TicketNotSent':
                    $ctrl.searchModel.isAttending = true;
                    $ctrl.searchModel.isMailed = false;
                    break;
                case 'TicketSent':
                    $ctrl.searchModel.isAttending = true;
                    $ctrl.searchModel.isMailed = true;
                    break;
                default:
            }
            $ctrl.search(tableStateRef);
        }

        $ctrl.addGuestTicket = function (e) {
            //TODO: show modal to add tickets
            e.additionalTickets = 2; 
            $http.post('api/event/' + $ctrl.eventId + '/addticket/', e).then(function (r) {
                var guest = r.data;
                guest.choices = buildGuestOptions(guest);
                angular.extend(e, guest);
                toastr.success('Added ' + e.additionalTickets + ' tickets to ' + guest.name);
            }).catch(function (err) {
                console.log('Oops. Something went wrong', err);
                toastr.error('Oops. Something went wrong addig tickets to guest', err.data.message);
            }); 
        }

        $ctrl.reservationOverride = function (e) {
            $http.post('api/event/' + $ctrl.eventId + '/register/', e).then(function (r) {
                var guest = r.data;
                guest.choices = buildGuestOptions(guest);
                angular.extend(e, guest);
                toastr.success('Registered ' + guest.name);
            }).catch(function (err) {
                console.log('Oops. Something went wrong', err);
                toastr.error('Oops. Something went wrong registering guest', err.data.message);
            }); 
        }

        $ctrl.cancelRegistration = function (e) {
            $http.post('api/event/' + $ctrl.eventId + '/cancelregister/' + e.id).then(function (r) {
                var guest = r.data;
                guest.choices = buildGuestOptions(guest);
                angular.extend(e, guest);
                toastr.success('Canceled registration for ' + guest.name);
            }).catch(function (err) {
                console.log('Oops. Something went wrong', err);
                toastr.error('Oops. Something went wrong cancelling registration', err.data.message);
            });
        }

        $ctrl.registerGuest = function (e) {
            e.ticketCount = 1;
            //TODO: edit modal shown here
            $http.post('api/event/' + $ctrl.eventId + '/register/', e).then(function (r) {
                var guest = r.data; 
                guest.choices = buildGuestOptions(guest);
                angular.extend(e, guest);
                toastr.success('Registered ' + guest.name);
            }).catch(function(err) {
                console.log('Oops. Something went wrong', err);
                toastr.error('Oops. Something went wrong registering guest', err.data.message);
            }); 
        }

        $ctrl.sendMail = function (e) {
            $http.post('api/event/' + $ctrl.eventId + '/mailticket/' + e.id).then(function (r) {
                var guest = r.data;
                guest.choices = buildGuestOptions(guest);
                angular.extend(e, guest);
                toastr.success('Mailed ticket for ' + guest.name);
            }).catch(function (err) {
                console.log('Oops. Something went wrong', err);
                toastr.error('Oops. Something went wrong mailing ticket', err.data.message);
            });
        }

        function buildGuestOptions(guest) {
            var options = [];

            if (guest.canRegister) options.push(choices[0]);
            if (guest.canMail) options.push(choices[1]);
            if (guest.canAddToAttending) options.push(choices[3]);

            if (guest.canAddTickets) options.push(choices[4]);
            if (guest.canCancel) options.push(choices[2]);

            guest.primaryChoice = angular.copy(options[0]);
            options.shift();
            return options;
        }

    }

    module.component('guestList',
        {
            bindings: {
                eventId: '<'
            },
            templateUrl: 'app/events/guest-list.component.html',
            controller: ['$http', 'toastr', controller]
        });

}
)();