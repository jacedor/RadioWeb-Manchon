var WDApp = function (login) {

    var self = this;
    self.ws = {};
    self.login = login;

    self.waitSeconds= function (iMilliSeconds) {
        var counter = 0
            , start = new Date().getTime()
            , end = 0;
        while (counter < iMilliSeconds) {
            end = new Date().getTime();
            counter = end - start;
        }
    }
    self.onopen = function () {
        $(".connStatus").html("<i class='fa fa-wifi fa-2x' title='conexión establecida con servidor' style='color:green;'></i>");
    };

    self.onmessage = function (event) {
      
        var returnAction = JSON.parse(event.data);
       
        if (returnAction.Fecha === $("#FILTROS_FECHA").val()) {
           
            var targetForm = $('form');
            var urlWithParams = targetForm.attr('action') + "?" + targetForm.serialize();
            self.waitSeconds(2000);
            $('#ExploracionesTable').bootstrapTable('refresh', {
                url: urlWithParams,
                silent: true
            });
        }
        

       
       // return false;
        //if (returnAction.Action == "new") {
        //    $(".note").find("ul").append("<li><span class=\"unchecked\"></span><span class=\"todo\">" + returnAction.Message + "</span><span class=\"delete\"></span></li>");
        //} else if (returnAction.Action == "check") {
        //    var item = $("ul").find("li:nth-child(" + (parseInt(returnAction.Message) + 1) + ")").find("span:first");
        //    item.removeClass("unchecked");
        //    item.addClass("checked");
        //} else if (returnAction.Action == "uncheck") {
        //    var item = $("ul").find("li:nth-child(" + (parseInt(returnAction.Message) + 1) + ")").find("span:first");
        //    item.removeClass("checked");
        //    item.addClass("unchecked");
        //} else if (returnAction.Action == "delete") {
        //    var item = $("ul").find("li:nth-child(" + (parseInt(returnAction.Message) + 1) + ")");
        //    item.remove();
        //}

    };

    self.onerror = function (evt) {
        $(".connStatus").html("<i class='fa fa-wifi' title='conexión no establecida con servidor' style='color:red:'></i>");
    };

    self.onclose = function (evt) {
        $(".connStatus").html("disconnected");
    };

    self.init = function () {
        var port = window.location.port;
        if ('WebSocket' in window) {
            
            self.ws = new WebSocket("wss://" + window.location.hostname + ":" + (port === "" ? "443" : port) + "/api/WebSocket?login=" + self.login);
           // self.ws = new WebSocket("ws://" + window.location.hostname + ":" + (port == "" ? "80" : port) + "/api/WebSocket?login=" + self.login);
        }
        else if ('MozWebSocket' in window) {
            self.ws = new MozWebSocket("wss://" + window.location.hostname + ":" + (port === "" ? "443" : port) + "/api/WebSocket?login=" + self.login);
        }
        else {
            return;
        }


       // $(".connStatus").html("connecting...");
        self.setupSocketEvents();
        self.setupDomEvents();

    };

    self.send = function (oidaparato, fecha,oidexploracion) {
        if (self.ws.readyState === WebSocket.OPEN) {
            var str = JSON.stringify({
                OidAparato: oidaparato,
                Fecha: fecha,
                oidExploracion: oidexploracion
            });
            self.ws.send(str);
        }
    };

    self.close = function () {
        self.ws.close();
    };

    self.setupDomEvents = function () {
    
        $(document.body).delegate(".WsAltaExploracion,button#btnActualizarPresencia,button#btnPresenciaImprimir,button#btnActualizarPresenciaEntrada,button#btnConfirmar"
            ,"click", function () {
                var row = $("#ExploracionesTable").find('tr.ACTIVA');
                if (row.length>0) {
                    self.send($("#FILTROS_IOR_APARATO").val(), $("#FILTROS_FECHA").val(), row.data('oid'));
                } else {
                    self.send($("#IOR_APARATO").val(),$("#FECHA").val(),-1);
                }               
        });
       
    };


    self.setupSocketEvents = function () {
        self.ws.onopen = function (evt) { self.onopen(evt); };
        self.ws.onmessage = function (evt) { self.onmessage(evt); };
        self.ws.onerror = function (evt) { self.onerror(evt); };
        self.ws.onclose = function (evt) { self.onclose(evt); };
    };
};