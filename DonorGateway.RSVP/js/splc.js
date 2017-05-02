$(document).ready(function() {

    var _splc = {

        toggleNav: function() {
            $('.nav-toggle').click(function(e) {
                e.preventDefault();
                $(this).toggleClass('open');
                $('.main-nav').slideToggle(200);
            });
        },

        toggleOtherAmount: function() {
            $('.donation-group .radio-btn label').click(function() {
                $('.other-group').fadeOut();
            })
            $('label.other-toggle').click(function(){
                $('.other-group').fadeToggle();
            });
        },

        toggleTribute: function() {
            $('.donation-options-group .radio-btn label').click(function() {
                $('.tribute-group').fadeOut();
            })
            $('label.tribute').click(function(){
                $('.tribute-group').fadeToggle();
            });
        },

        init: function() {
            this.toggleNav();
            this.toggleOtherAmount();
            this.toggleTribute();
        }
    };

    _splc.init();

});
