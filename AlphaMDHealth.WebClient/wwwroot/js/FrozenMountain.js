let audio = false;
let video = false;
let localMedia;
let client;
let channelId;
//for frozen mountain
async function RegisterFrozenMountain(applicationID, userID, channelName, registerToken, token, url) {

    var status = true;
    channelId = channelName;
    client = new fm.liveswitch.Client(url, applicationID, userID, '');
   
    client.register(registerToken).then(function (channels) {
        console.log('Registered successfully!!');
        client.join(channelId, token).then((channel) => {
            console.log('Successfully joined a channel');


            // handle the media implementation here
            const audio = true;
            // handle the canvas
            var video = true;
            // instance to capture the local media
            localMedia = new fm.liveswitch.LocalMedia(audio, video);
            // create instance to capture the remote media
            const remoteMedia = new fm.liveswitch.RemoteMedia();
            const audioStream = new fm.liveswitch.AudioStream(localMedia, remoteMedia);
            const videoStream = new fm.liveswitch.VideoStream(localMedia, remoteMedia);

            const mcuConnection = channel.createMcuConnection(audioStream, videoStream);

            const layoutManager = new fm.liveswitch.DomLayoutManager(document.getElementById('camera'));


            localMedia.start().then((lm) => {
                console.log('started recording');
                // render the captured video into the container
                layoutManager.setLocalView(localMedia.getView());

                layoutManager.addRemoteView(remoteMedia.getId(), remoteMedia.getView());
                mcuConnection.setIceServers([
                    new fm.liveswitch.IceServer('stun:turn.frozenmountain.com:3478'),
                    new fm.liveswitch.IceServer('turn:turn.frozenmountain.com:80', 'test', 'pa55w0rd!'),
                    new fm.liveswitch.IceServer('turns:turn.frozenmountain.com:443', 'test', 'pa55w0rd!')
                ]);
                try {
                    mcuConnection.open()
                        .then(result => console.log('Mixed connection established'))
                        .fail(err => console.log('MCU Open connection Error', err.message));
                } catch (err) {
                    console.log(err.message);
                }
                status = true;
                // layoutManager.unsetLocalView();
            }).fail(err => { console.log('Some syntax error: ', err.message); return false; });
            // localMedia.start().then(lm => console.log(lm)).catch(err => console.log(err.message));
        }).fail(err => { console.log('Failed while joining a channel', err); status = false; });
    }).fail(err => { console.log('not registered', err); status = false; });
    return status;
};

async function MuteAudioAsync() {
    localMedia.setAudioMuted(true);
}

async function UnMuteAudioAsync() {
    localMedia.setAudioMuted(false);
}

async function MuteVideoAsync() {
    console.log(localMedia.getVideoMuted());
    localMedia.setVideoMuted(true);
    console.log(localMedia.getVideoMuted());
}

async function UnMuteVideoAsync() {
    console.log(localMedia.getVideoMuted());
    localMedia.setVideoMuted(false);
    console.log(localMedia.getVideoMuted());
}


function LeaveAsync() {
    var status = true;
    client.leave(channelId).then(function (channel) {
        localMedia.stop()
        localMedia = null;
        console.log("left the channel");
        status = true;
    }).fail(function (ex) {
        console.log("failed to leave the channel");
        status = false;
    });
    client.unregister().then(function (result) {
        console.log("unregistration succeeded");
        status = true;
    }).fail(function (ex) {
        console.log("unregistration failed");
        status = false;
    });
    return status;
}

window.videoInterops = {
    RegisterFrozenMountain,
    MuteAudioAsync,
    UnMuteAudioAsync,
    MuteVideoAsync,
    UnMuteVideoAsync,
    LeaveAsync
};
