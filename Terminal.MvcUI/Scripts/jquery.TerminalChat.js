(function ($) {

    var defaults = {
    };

    $.fn.terminalChat = function (options) {

        var $chatContainer = this,
            $chatCli,
            $chatDisplay,
            settings = $.extend({}, defaults, options);

        var writeLine = function (text) {
            $('<div>')
                .text(text)
                .appendTo($chatDisplay);
        };

        var chatHub = $.signalR.chatHub;

        chatHub.writeLine = function (text) {
            writeLine(text);
            $chatContainer.scrollTo('100%', 0, { axis: 'y' });
        };

        $chatDisplay = $('<div>').appendTo($chatContainer);

        $chatCli = $('<input type="text" />')
           .addClass('cli')
           .keypress(function (e) {
               e.stopPropagation();
               var key = e.keyCode ? e.keyCode : e.charCode;
               if (key == 13) {
                   chatHub.send($(this).val());
                   $(this).val('');
               }
           });

        $('<div>')
            .css({ marginTop: '10px' })
            .append('&gt;&nbsp;')
            .append($chatCli)
            .appendTo($chatContainer);

        $.connection.hub.start({ transport: 'longPolling' }, function () {
            //chatHub.connectUser(settings.username);
        });

        // Temporary workaround to interact with terminal client.
        $(document).bind('userLoggedIn', function (e, username) { chatHub.connectUser(username); });
        $(document).bind('userLoggedOut', function () { chatHub.disconnectUser(); });

    };
})(jQuery);