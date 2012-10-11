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

        var terminalHub = $.signalR.terminalHub;

        terminalHub.joinUser = function (username) {
            writeLine('<b>' + username + '</b> has joined.');
        };

        terminalHub.leaveUser = function (username) {
            writeLine('<b>' + username + '</b> has left.');
        };

        terminalHub.message = function (text) {
            writeLine(text);
        };

        terminalHub.writeLine = function (username, text) {
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
                       terminalHub.send(text);
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
            $(document).bind('userLoggedIn', function () {
                $.ajax({
                    url: '/Api/ConnectUser',
                    data: { connectionId: $.connection.hub.id },
                    type: 'post'
                });
            });
            $(document).bind('userLoggedOut', function () { terminalHub.disconnectUser(); });

            $.ajax({
                url: '/Api/ConnectUser',
                data: { connectionId: $.connection.hub.id },
                type: 'post'
            });

            if (settings.callback)
                settings.callback();
        });
    };
})(jQuery);