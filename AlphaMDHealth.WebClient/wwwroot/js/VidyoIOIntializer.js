function loadVidyoClientLibrary(webrtc, plugin) {
    // If webrtc, then set webrtcLogLevel
    var webrtcLogLevel = "";
    if (webrtc) {
        // Set the WebRTC log level to either: 'info' (default), 'error', or 'none'
        if (configParams.webrtcLogLevel === 'info' || configParams.webrtcLogLevel === 'error' || configParams.webrtcLogLevel == 'none')
            webrtcLogLevel = '&webrtcLogLevel=' + configParams.webrtcLogLevel;
        else
            webrtcLogLevel = '&webrtcLogLevel=info';
    }
    //We need to ensure we're loading the VidyoClient library and listening for the callback.
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = 'https://static.vidyo.io/latest/javascript/VidyoClient/VidyoClient.js?onload=onVidyoClientLoaded&webrtc=' + webrtc + '&plugin=' + plugin /*+ webrtcLogLevel*/;
    document.getElementsByTagName('head')[0].appendChild(script);
}


function joinViaPlugIn() {
    $("#helperText").html("Don't have the PlugIn?");
    $("#helperPicker").addClass("hidden");
    $("#helperPlugIn").removeClass("hidden");
    loadVidyoClientLibrary(true, false);
}

function loadPlatformInfo(platformInfo) {
    var userAgent = navigator.userAgent || navigator.vendor || window.opera;
    // Opera 8.0+
    platformInfo.isOpera = userAgent.indexOf("Opera") != -1 || userAgent.indexOf('OPR') != -1;
    // Firefox
    platformInfo.isFirefox = userAgent.indexOf("Firefox") != -1 || userAgent.indexOf('FxiOS') != -1;
    // Chrome 1+
    platformInfo.isChrome = userAgent.indexOf("Chrome") != -1 || userAgent.indexOf('CriOS') != -1;
    // Safari
    platformInfo.isSafari = !platformInfo.isFirefox && !platformInfo.isChrome && userAgent.indexOf("Safari") != -1;
    // AppleWebKit
    platformInfo.isAppleWebKit = !platformInfo.isSafari && !platformInfo.isFirefox && !platformInfo.isChrome && userAgent.indexOf("AppleWebKit") != -1;
    // Internet Explorer 6-11
    platformInfo.isIE = (userAgent.indexOf("MSIE") != -1) || (!!document.documentMode == true);
    // Edge 20+
    platformInfo.isEdge = !platformInfo.isIE && !!window.StyleMedia;
    // Check if Mac
    platformInfo.isMac = navigator.platform.indexOf('Mac') > -1;
    // Check if Windows
    platformInfo.isWin = navigator.platform.indexOf('Win') > -1;
    // Check if Linux
    platformInfo.isLinux = navigator.platform.indexOf('Linux') > -1;
    // Check if iOS
    platformInfo.isiOS = userAgent.indexOf("iPad") != -1 || userAgent.indexOf('iPhone') != -1;
    // Check if Android
    platformInfo.isAndroid = userAgent.indexOf("android") > -1;
    // Check if Electron
    platformInfo.isElectron = (typeof process === 'object') && process.versions && (process.versions.electron !== undefined);
    // Check if WebRTC is available
    platformInfo.isWebRTCAvailable = (navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMedia || (navigator.mediaDevices ? navigator.mediaDevices.getUserMedia : undefined)) ? true : false;
    // Check if 64bit
    platformInfo.is64bit = navigator.userAgent.indexOf('WOW64') > -1 || navigator.userAgent.indexOf('Win64') > -1 || window.navigator.platform == 'Win64';
}


function OnPageLoad (roomName, token, userId, url) {
    var connectorType = getUrlParameterByName("connectorType");
    loadPlatformInfo(platformInfo);
    // Extract the desired parameter from the browser's location bar
    function getUrlParameterByName(name) {
        var match = RegExp('[?&]' + name + '=([^&]*)').exec(window.location.search);
        return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
    }

    // Fill in the form parameters from the URI
    var host = url;
    var token = token;
    var displayName = userId;
    var resourceId = roomName;
    configParams.autoJoin = getUrlParameterByName("autoJoin");
    configParams.enableDebug = getUrlParameterByName("enableDebug");
    configParams.microphonePrivacy = getUrlParameterByName("microphonePrivacy");
    configParams.cameraPrivacy = getUrlParameterByName("cameraPrivacy");
    configParams.speakerPrivacy = getUrlParameterByName("speakerPrivacy");
    configParams.webrtcLogLevel = getUrlParameterByName("webrtcLogLevel");
    configParams.returnURL = getUrlParameterByName("returnURL");
    configParams.isIE = platformInfo.isIE;
    var hideConfig = getUrlParameterByName("hideConfig");

    // If the parameters are passed in the URI, do not display options dialog
    if (host && token && displayName && resourceId) {
        $(".optionsParameters").addClass("hiddenPermanent");
    }

    if (hideConfig == "1") {
        $("#options").addClass("hiddenPermanent");
        $("#optionsVisibilityButton").addClass("hiddenPermanent");
        $("#renderer").addClass("rendererWithoutOptionsPermanent");
    }
    joinViaPlugIn();
}

function onVidyoClientLoaded(status) {
    console.log("Status: " + status.state + "Description: " + status.description);
    switch (status.state) {
        case "READY":    // The library is operating normally
            $("#connectionStatus").html("Ready to Connect");
            $("#helper").addClass("hidden");
            $("#optionsVisibilityButton").removeClass("hidden");
            $("#renderer").removeClass("hidden");
            $("#toolbarLeft").removeClass("hidden");
            $("#toolbarCenter").removeClass("hidden");
            $("#toolbarRight").removeClass("hidden");

            // If configured to autoJoin, then show video full screen immediately
            if (configParams.autoJoin === "1") {
                $("#optionsVisibilityButton").addClass("showOptions").removeClass("hideOptions");
                $("#renderer").addClass("rendererWithoutOptions").removeClass("rendererWithOptions");
            } else
                $("#options").removeClass("hidden");

            // If WebRTC is being used, specify the screen share extension path.
            if (VCUtils.params.webrtc === "true") {
                if (status.hasOwnProperty("downloadPathWebRTCExtensionFirefox"))
                    webrtcExtensionPath = status.downloadPathWebRTCExtensionFirefox;
                else if (status.hasOwnProperty("downloadPathWebRTCExtensionChrome"))
                    webrtcExtensionPath = status.downloadPathWebRTCExtensionChrome;
            }

            // Determine which Vidyo stack will be used: Native WebRTC, Transcoding WebRTC or Native (Plugin/Electron)
            var useTranscodingWebRTC, performMonitorShare;
            if (status.description.indexOf("Native XMPP + WebRTC") > -1) {
                // Native WebRTC
                useTranscodingWebRTC = false;
                performMonitorShare = false;
            } else if (status.description.indexOf("WebRTC successfully loaded") > -1) {
                // Transcoding WebRTC
                useTranscodingWebRTC = true;
                performMonitorShare = false;
                ++webrtcInitializeAttempts;
            } else {
                // Native (Plugin or Electron)
                useTranscodingWebRTC = false;
                performMonitorShare = true;
            }

            // After the VidyoClient is successfully initialized a global VC object will become available
            // All of the VidyoConnector gui and logic is implemented in VidyoConnector.js
            StartVidyoConnector(VC, useTranscodingWebRTC, performMonitorShare, webrtcExtensionPath, configParams);

            break;
        case "RETRYING": // The library operating is temporarily paused
            $("#connectionStatus").html("Temporarily unavailable retrying in " + status.nextTimeout / 1000 + " seconds");
            break;
        case "FAILED":   // The library operating has stopped
            // If WebRTC initialization failed, try again up to 3 times.
            if ((status.description.indexOf("Could not initialize WebRTC transport") > -1) && (webrtcInitializeAttempts < 3)) {
                // Attempt to start the VidyoConnector again.
                StartVidyoConnector(VC, VCUtils.params.webrtc, webrtcExtensionPath, configParams);
                ++webrtcInitializeAttempts;
            } else {
                ShowFailed(status);
            }
            break;
        case "FAILEDVERSION":   // The library operating has stopped
            UpdateHelperPaths(status);
            ShowFailedVersion(status);
            $("#connectionStatus").html("Failed: " + status.description);
            break;
        case "NOTAVAILABLE": // The library is not available
            UpdateHelperPaths(status);
            $("#connectionStatus").html(status.description);
            break;
        case "TIMEDOUT":   // Transcoding Inactivity Timeout
            $("#connectionStatus").html("Failed: " + status.description);
            $("#messages #error").html('Page timed out due to inactivity. Please refresh your browser and try again.');
            break;
    }
    return true; // Return true to reload the plugins if not available
}



window.videoIOInterop = {
    OnPageLoad,
};