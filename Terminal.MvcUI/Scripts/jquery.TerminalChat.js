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
                .html(text)
                .appendTo($chatDisplay);
            $chatContainer.scrollTo('100%', 0, { axis: 'y' });
        };

        var chatHub = $.signalR.chatHub;

        chatHub.joinUser = function (username) {
            writeLine('<b>' + username + '</b> has joined.');
        };

        chatHub.leaveUser = function (username) {
            writeLine('<b>' + username + '</b> has left.');
        };

        chatHub.message = function (text) {
            writeLine(text);
        };

        chatHub.writeLine = function (username, text) {
            writeLine('<b>{' + username + '}</b>&nbsp;' + text);
        };

        $chatDisplay = $('<div>').appendTo($chatContainer);

        $chatCli = $('<input type="text" />')
           .addClass('cli')
           .keypress(function (e) {
               e.stopPropagation();
               var key = e.keyCode ? e.keyCode : e.charCode;
               if (key == 13) {
                   var text = $(this).val();
                   if (text.length > 0) {
                       chatHub.send(text);
                       $(this).val('');
                   }
               }
           });

        $('<div>')
            .css({ marginTop: '10px' })
            .append('&gt;&nbsp;')
            .append($chatCli)
            .appendTo($chatContainer);

        $.connection.hub.start({ transport: 'longPolling' }, function () {
            // Temporary workaround to interact with terminal client.
            $(document).bind('userLoggedIn', function (e, username) { chatHub.connectUser(username); });
            $(document).bind('userLoggedOut', function () { chatHub.disconnectUser(); });

            if (settings.callback)
                settings.callback();
        });
    };
})(jQuery);