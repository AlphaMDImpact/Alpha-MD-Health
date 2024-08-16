function FileSaveAs(filename, fileContent) {
    if (fileContent == undefined) {
        window.open(filename, "_blank")
    }
    else {
        var link = document.createElement('a');
        link.download = filename;
        link.href = fileContent;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
}

let _videoTrack = null;
let _activeRoom = null;
let _participants = new Map();
let _dominantSpeaker = null;
let _session = null;

async function getVideoDevices() {
    try {
        let devices = await navigator.mediaDevices.enumerateDevices();
        if (devices &&
            (devices.length === 0 || devices.every(d => d.deviceId === ""))) {
            await navigator.mediaDevices.getUserMedia({
                video: true
            });
        }

        devices = await navigator.mediaDevices.enumerateDevices();
        if (devices && devices.length) {
            const deviceResults = [];
            devices.filter(device => device.kind === 'videoinput')
                .forEach(device => {
                    const { deviceId, label } = device;
                    deviceResults.push({ deviceId, label });
                });

            return deviceResults;
        }
    } catch (error) {
        console.log(error);
    }

    return [];
}

async function startVideo(deviceId, selector) {
    const cameraContainer = document.querySelector(selector);
    if (!cameraContainer) {
        return;
    }

    try {
        if (_videoTrack) {
            _videoTrack.detach().forEach(element => element.remove());
        }

        _videoTrack = await Twilio.Video.createLocalVideoTrack({ deviceId });
        const videoEl = _videoTrack.attach();
        cameraContainer.append(videoEl);
    } catch (error) {
        console.log(error);
    }
}

function waitForSession() {
    return new Promise((resolve, reject) => {
        let y = 0
        setTimeout(() => {
            while (_videoTrack == undefined || _videoTrack == null) {

            }
            console.log('resolved completed')
            resolve(y)
        }, 2000)
    })
}

async function createOrJoinRoom(roomName, token) {
    try {
        if (_activeRoom) {
            _activeRoom.disconnect();
        }
        if (_videoTrack === undefined || _videoTrack === null) {
            await waitForSession();
            await createOrJoinRoom(roomName, token);
        }
        else {
            const audioTrack = await Twilio.Video.createLocalAudioTrack();
            const tracks = [audioTrack, _videoTrack];
            _activeRoom = await Twilio.Video.connect(
                token, {
                name: roomName,
                tracks,
                dominantSpeaker: true,
            });
            document.getElementById("participants").classList.add("d-flex");
            document.getElementById("participants").classList.add("flex-wrap");
            if (_activeRoom) {
                initialize(_activeRoom.participants);
                _activeRoom
                    .on('disconnected',
                        room => room.localParticipant.tracks.forEach(
                            publication => detachTrack(publication.track)))
                    .on('participantConnected', participant => add(participant))
                    .on('participantDisconnected', participant => remove(participant))
                    .on('dominantSpeakerChanged', dominantSpeaker => loudest(dominantSpeaker));
            }
        }
    } catch (error) {
        console.error(`Unable to connect to Room: ${error.message}`);
    }
    return !!_activeRoom;
}

function initialize(participants) {
    _participants = participants;
    if (_participants) {
        _participants.forEach(participant => registerParticipantEvents(participant));
    }
}

function add(participant) {

    if (_participants && participant) {
        _participants.set(participant.sid, participant);
        registerParticipantEvents(participant);
    }
}

function remove(participant) {
    if (_participants && _participants.has(participant.sid)) {
        _participants.delete(participant.sid);
    }
}

function loudest(participant) {
   
    removeDominantSpeaker();
    if (participant !== null) {
        let asdocimanr = participant.videoTracks.entries().next().value;
        let sID = asdocimanr[0];
        assignDominantSpeaker(sID);
    }
    
}

function removeDominantSpeaker() {
    if (_dominantSpeaker !== null) {
        document.getElementById(_dominantSpeaker).style.border = "none";
        _dominantSpeaker = null;
    }   
}

function assignDominantSpeaker(sidb) {
    _dominantSpeaker = sidb;
    document.getElementById(sidb).style.border = "5px solid red";
}

function registerParticipantEvents(participant) {
    if (participant) {
        participant.tracks.forEach(publication => subscribe(publication));
        participant.on('trackPublished', publication => subscribe(publication));
        participant.on('trackUnpublished',
            publication => {
                if (publication && publication.track) {
                    detachRemoteTrack(publication.track);
                }
            });
    }
}

function subscribe(publication) {
    if (isMemberDefined(publication, 'on')) {
        publication.on('subscribed', track => attachTrack(track));
        publication.on('unsubscribed', track => detachTrack(track));
    }
}

function attachTrack(track) {
    if (isMemberDefined(track, 'attach')) {
        const audioOrVideo = track.attach();
        audioOrVideo.id = track.sid;

        if ('video' === audioOrVideo.tagName.toLowerCase()) {
            const responsiveDiv = document.createElement('div');
            responsiveDiv.id = track.sid;
            if (document.getElementsByClassName('embed-responsive').length >= 10) {
                const responsiveItem = document.createElement('div');
                responsiveDiv.classList.add('child-videos');
                responsiveDiv.classList.add('margin-horizontal-xs');
                responsiveItem.classList.add('child-videos');
                audioOrVideo.classList.add('child-videos');
                responsiveItem.appendChild(audioOrVideo);
                responsiveDiv.appendChild(responsiveItem);
                document.getElementById('participants-min').appendChild(responsiveDiv); 
            }
            else {
                responsiveDiv.classList.add('embed-responsive');
                responsiveDiv.classList.add('embed-responsive-16by9');
                responsiveDiv.classList.add('video-border-top');
                const responsiveItem = document.createElement('div');
                responsiveItem.classList.add('embed-responsive-item');
                responsiveItem.appendChild(audioOrVideo);
                responsiveDiv.appendChild(responsiveItem);
                document.getElementById('participants').appendChild(responsiveDiv);
                adjustViews();
            }
           
        }

        else {

            document.getElementById('participants')
                .appendChild(audioOrVideo);
        }
    }
}


function adjustViews() {
    let reposnvie = document.getElementById('camera');
    if (document.getElementsByClassName('embed-responsive').length > 1) {
        reposnvie.parentElement.classList.add('local-side-track');
        reposnvie.parentElement.classList.add('margin-horizontal-sm');
    }
    else {
        reposnvie.parentElement.classList.remove('local-side-track');
        reposnvie.parentElement.classList.add('margin-horizontal-sm');
    }
    let documentUas = document.getElementsByClassName('embed-responsive');
    let viewWidth;
    let viewHeight;
    if (documentUas.length <= 2) {
        viewHeight = "100%";
        viewWidth = "100%";
    }
    else if (documentUas.length > 7) {
        viewHeight = "33.33%";
        viewWidth = "33.33%";
    }
    else if (documentUas.length > 5) {
        viewHeight = "50%";
        viewWidth = "33.33%";
    }
    else if (documentUas.length > 2) {
        viewHeight = "100%";
        viewWidth = "50%";
    }
    
    
    for (var i = 0; i < documentUas.length; i++) {
        if (!documentUas[i].classList.contains('local-side-track')) {
            documentUas[i].style.width = viewWidth;
            documentUas[i].style.height = viewHeight;
        }
    }
}

function detachTrack(track) {
    if (this.isMemberDefined(track, 'detach')) {
        track.detach()
            .forEach(el => {
                if ('video' === el.tagName.toLowerCase()) {
                    const parent = el.parentElement;
                    if (parent && parent.id !== 'camera') {
                        const grandParent = parent.parentElement;
                        if (grandParent) {
                            grandParent.remove();
                        }
                    }
                } else {
                    el.remove()
                }
            });
    }
    adjustViews();
}

function isMemberDefined(instance, member) {
    return !!instance && instance[member] !== undefined;
}

async function leaveRoom() {

    try {
      
        if (_activeRoom) {
            var localParticipant = _activeRoom.localParticipant;
            localParticipant.videoTracks.forEach(function (videoTrack) {
                videoTrack.track.stop();

            });
            localParticipant.audioTracks.forEach(function (audioTrack) {
                audioTrack.track.stop();

            });
            _activeRoom.disconnect();
            _activeRoom = null;
        }

        if (_participants) {
            _participants.clear();
        }
    }
    catch (error) {
        console.error(error);
    }
}

async function muteVideo() {
    var localParticipant = _activeRoom.localParticipant;
    localParticipant.videoTracks.forEach(function (videoTrack) {
        videoTrack.track.disable();

    });
}

async function unMuteVideo() {
    var localParticipant = _activeRoom.localParticipant;
    localParticipant.videoTracks.forEach(function (videoTrack) {
        videoTrack.track.enable();
    });
}

async function unMuteAudio() {
    var localParticipant = _activeRoom.localParticipant;
    localParticipant.audioTracks.forEach(function (audioTrack) {
        audioTrack.track.enable();
    });
}

async function muteAudio() {
    var localParticipant = _activeRoom.localParticipant;
    localParticipant.audioTracks.forEach(function (audioTrack) {
        audioTrack.track.disable();
    });
}




var _OTSession;
var currentpublisher;
var currentstreams=[];

async function initializeSession(apikey, roomName, token) {
    try {
        // Initialize an OpenTok Session object
        //_OTSession = null;
        _OTSession = OT.initSession(apikey, roomName); //video resolution

        var publisherOptions = {

            insertMode: "append",

            height: "100%",

            width: "100%"
        }
        document.getElementById('participants').classList.add("d-flex");
        document.getElementById('participants').classList.add("flex-wrap");
        // Initialize a Publisher, and place it into the element with id="publisher"

        var publisher = OT.initPublisher('camera', publisherOptions);

        // Attach event handlers

        _OTSession.on({
            // This function runs when session.connect() asynchronously completes

            sessionConnected: function (event) {
                _OTSession.publish(publisher, function (error) {

                    if (error) {
                        console.error('Failed to publish', error);

                    }
                });
                

            },     // This function runs when another client publishes a stream (eg. session.publish())
            sessionDisconnected: function (event) {
               
               
            },
            streamDestroyed: function (event) {
                let streamId = event.stream.streamId
                if (streamId !== null) {
                    let subs = _OTSession.getSubscribersForStream(streamId);
                    _OTSession.unsubscribe(subs);
                    let devToBeDestroyed = document.getElementById(streamId);
                    devToBeDestroyed.remove();
                    adjustViews();
                    const indx = currentstreams.findIndex(x => x.streamId === event.stream.streamId);
                    currentstreams.splice(indx, indx >= 0 ? 1 : 0);
                }
               
            },
            streamCreated: function (event) {
                // Create a container for a new Subscriber, assign it an id using the streamId, put it inside
                let existing = undefined;
                // the element with id="subscribers"
                if (currentstreams !== null && currentstreams.length > 0) {
                    existing = currentstreams.find(x => x.streamId === event.stream.streamId);
                }
                if (existing === undefined) {
                    let conatin = document.createElement('div');
                    conatin.classList.add("embed-responsive");
                    conatin.classList.add("embed-responsive-16by9");
                    let conatinSub = document.createElement('div');
                    conatinSub.classList.add("embed-responsive-item");
                    var subContainer = document.createElement('div');
                    currentstreams.push(event.stream);
                    subContainer.id = 'stream-' + event.stream.streamId;
                    conatinSub.appendChild(subContainer);
                    conatin.appendChild(conatinSub);
                    conatin.id = event.stream.streamId;
                    if (_OTSession.streams.length() < 10) {
                        document.getElementById('participants').appendChild(conatin);         // Subscribe to the stream that caused this event, put it inside the publisher container
                    } else {
                        document.getElementById('participants-min').appendChild(subContainer);
                    }

                    var subscriber = _OTSession.subscribe(event.stream, subContainer, function (error) {
                        let documentUas = document.getElementsByClassName('embed-responsive');
                        for (var i = 0; i < documentUas.length; i++) {
                            if (documentUas[i].id !== "") {
                                documentUas[i].children[0].children[0].style.height = '100%';
                                documentUas[i].children[0].children[0].style.width = '100%';
                                //documentUas[i].classList.add("embed-responsive");
                            }
                        }
                        adjustViews();
                        if (error) {

                            console.error('Failed to subscribe', error);

                        }

                    });

                    var movingAvg = null;
                    subscriber.on('audioLevelUpdated', function (event) {
                        if (movingAvg === null || movingAvg <= event.audiolevel || event.audioLevel === 0) {
                            movingAvg = event.audioLevel;
                            document.getElementById(event.target.id).style.border = "none";
                        } else {

                            movingAvg = 0.7 * movingAvg + 0.3 * event.audioLevel;
                            document.getElementById(event.target.id).style.border = "2px solid red";
                        }

                        // 1.5 scaling to map the -30 - 0 dBm range to [0,1]
                        //var logLevel = (Math.log(movingAvg) / Math.LN10) / 1.5 + 1;
                        //logLevel = Math.min(Math.max(logLevel, 0), 1);
                        //document.getElementById('subscriberMeter').value = logLevel;
                    });
                    subscriber.on('disconnected', function (event) {

                        // 1.5 scaling to map the -30 - 0 dBm range to [0,1]
                        //var logLevel = (Math.log(movingAvg) / Math.LN10) / 1.5 + 1;
                        //logLevel = Math.min(Math.max(logLevel, 0), 1);
                        //document.getElementById('subscriberMeter').value = logLevel;
                    });

                    subscriber.on('destroyed', function (event) {

                        // 1.5 scaling to map the -30 - 0 dBm range to [0,1]
                        //var logLevel = (Math.log(movingAvg) / Math.LN10) / 1.5 + 1;
                        //logLevel = Math.min(Math.max(logLevel, 0), 1);
                        //document.getElementById('subscriberMeter').value = logLevel;
                    });
                }
            }
        
        }); // Connect to the Session using the 'apiKey' of the application and a 'token' for permission

        _OTSession.connect(token, function (error) {
            currentpublisher = publisher;
            if (error) {

                console.error('Failed to connect', error);

            }

        });


        return true;
    }
    catch (error) {
        console.error(error);
        //return false
    }
}



function muteVideoOTAsync() {
    currentpublisher.publishVideo(false);
}
function unmuteVideoOTAsync() {
    currentpublisher.publishVideo(true);
}
function muteAudioOTAsync() {
    currentpublisher.publishAudio(false);
}
function unmuteAudioOTAsync() {
    currentpublisher.publishAudio(true);
}

function leaveOTAsync() {
    currentstreams.forEach((streamy) => {
        let subs = _OTSession.getSubscribersForStream(streamy);
        subs.forEach(e => {
            _OTSession.unsubscribe(e);
        })
    })
    currentstreams = [];
    currentpublisher.publishVideo(false);
    currentpublisher.publishAudio(false);
    currentpublisher.disconnect();
    _OTSession.unpublish(currentpublisher);
    _OTSession.disconnect();
    _OTSession.destroy();
    _OTSession = null;
    currentpublisher = null;
    //currentpublisher.publishVideo(false);
    //currentpublisher.publishAudio(false);
    //currentpublisher.disconnect();
    return true;
}




async function handleError(error) {
    if (error) {
        alert(error.message);
    }
}

async function disconnect() {
    _session.disconnect();
}

window.videoInterop = {
    getVideoDevices,
    startVideo,
    createOrJoinRoom,
    leaveRoom,
    muteAudio,
    unMuteAudio,
    muteVideo,
    initializeSession,
    unMuteVideo,
    leaveOTAsync,
    muteVideoOTAsync,
    unmuteVideoOTAsync,
    muteAudioOTAsync,
    unmuteAudioOTAsync
};

window.store = {
    get: key => window.localStorage[key],
    set: (key, value) => window.localStorage[key] = value,
    delete: key => delete window.localStorage[key]
};