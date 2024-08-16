window.blazorSummernote = {


    init: function (elementId, heightValue, allowImage, dotNetReference) {
        $('#' + elementId).summernote({
            tabsize: 2,
            height: heightValue,
            dialogsInBody: true,
            toolbar: [
                ['fontname', ['fontname']],
                ['fontsize', ['fontsize']],
                ['fontstyle', ['bold', 'italic', 'underline', 'strikethrough', 'clear']],
                ['color', ['color']],
                ['style', ['style']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['table', ['table']],
                ['insert', ['link', allowImage == true ? 'picture' : '']],
                ['view', ['fullscreen', 'codeview']]
            ],
            callbacks: {
                onImageUpload: function (imagedata) {
                        $('#' + elementId).summernote('restoreRange');
                    $('#' + elementId).summernote('insertImage', imagedata);
                }
            }

        });
        $(document).on('click', 'button[aria-label="Picture"]', function (e) {
            $('[aria-label="Insert Image"]').hide();
            $(".note-modal-backdrop").hide();
            dotNetReference.invokeMethodAsync('ShowPictureModal', true);
        });
    },

    imageupload: function (elementId, imagedata) {
        $('#' + elementId).summernote('triggerEvent', 'imageUpload', imagedata);
    },
    getContent: function (elementId, value) {
        return $('#' + elementId).summernote('code', value);
    },

    setupTextChange: function (elementId, dotNetReference) {
        $('#' + elementId).on('summernote.change', function (we, contents, $editable) {
            dotNetReference.invokeMethodAsync('HandleTextChange', contents);
        });
    }
}
