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

async function createOrJoinRoom(roomName, token) {
    try {
        if (_activeRoom) {
            _activeRoom.disconnect();
        }

        const audioTrack = await Twilio.Video.createLocalAudioTrack();
        const tracks = [audioTrack, _videoTrack];
        _activeRoom = await Twilio.Video.connect(
            token, {
            name: roomName,
            tracks,
                dominantSpeaker: true,
        });

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
    _dominantSpeaker = participant;
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
            responsiveDiv.classList.add('embed-responsive');
            responsiveDiv.classList.add('embed-responsive-16by9');

            const responsiveItem = document.createElement('div');
            responsiveItem.classList.add('embed-responsive-item');

            // Similar to.
             //<div class="embed-responsive embed-responsive-16by9">
             //  <div id="camera" class="embed-responsive-item">
             //    <video></video>
             //  </div>
             //</div>
            responsiveItem.appendChild(audioOrVideo);
            responsiveDiv.appendChild(responsiveItem);
            if (document.getElementById('participants').children.length > 4) {
                document.getElementById('participants-min').appendChild(responsiveDiv);
            }
            else {
                document.getElementById('participants').appendChild(responsiveDiv);
            }
          
        } else {
            document.getElementById('participants')
                .appendChild(audioOrVideo);
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
}

function isMemberDefined(instance, member) {
    return !!instance && instance[member] !== undefined;
}

async function leaveRoom() {

    try {
        if (_activeRoom) {
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
    localParticipant.videoTracks.forEach(function (videoTracks) {
        videoTracks.track.disable();

    });
}

async function unMuteVideo() {
    var localParticipant = _activeRoom.localParticipant;
    localParticipant.videoTracks.forEach(function (videoTracks) {
        videoTracks.track.enable();
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


async function initializeSession(apikey, roomName, token) {
    try {
        var connectionCount;
        _session = OT.initSession(apikey, roomName);

        _session.on({
            connectionCreated: function (event) {
                connectionCount++;
                console.log(connectionCount + ' connections.');
            },
            connectionDestroyed: function (event) {
                connectionCount--;
                console.log(connectionCount + ' connections.');
            },
            sessionDisconnected: function sessionDisconnectHandler(event) {
                // The event is defined by the SessionDisconnectEvent class
                console.log('Disconnected from the session.');
                document.getElementById('disconnectBtn').style.display = 'none';
                if (event.reason == 'networkDisconnected') {
                    alert('Your network connection terminated.')
                }
            },

            sessionConnected: function (event) {
                // Publish the publisher we initialzed earlier (this will trigger 'streamCreated' on other
                // clients)
                _session.publish(publisher, function (error) {
                    if (error) {
                        console.error('Failed to publish', error);
                    }
                });
            },

            // This function runs when another client publishes a stream (eg. session.publish())
            streamCreated: function (event) {
                // Create a container for a new Subscriber, assign it an id using the streamId, put it inside
                // the element with id="subscribers"
                var subContainer = document.createElement('div');
                subContainer.id = 'stream-' + event.stream.streamId;
                document.getElementById('subscribers').appendChild(subContainer);

                // Subscribe to the stream that caused this event, put it inside the container we just made
                _session.subscribe(event.stream, subContainer, function (error) {
                    if (error) {
                        console.error('Failed to subscribe', error);
                    }
                });
            }
        });

        // Connect to the session
        _session.on('streamCreated', function (event) {
            _session.subscribe(event.stream, 'subscriber', {
                insertMode: 'append',
                width: '100%',
                height: '100%'
            }, handleError);
        });

        // Create a publisher
        var publisher = OT.initPublisher('publisher', {
            insertMode: 'append',
            width: '100%',
            height: '100%'
        }, handleError);
        _session.connect(token, function (error) {
            // If the connection is successful, initialize a publisher and publish to the session
            if (error) {
                handleError(error);
            } else {
                _session.publish(publisher, handleError);
            }
        });
    }
    catch (error) {
        console.error(error);
    }
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
    unMuteVideo
};

window.store = {
    get: key => window.localStorage[key],
    set: (key, value) => window.localStorage[key] = value,
    delete: key => delete window.localStorage[key]
};