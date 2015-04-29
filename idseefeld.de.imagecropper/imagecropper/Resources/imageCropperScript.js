/* imageCropperScript */
function initImageCropper(imageId, jsonId, rawId, scaleFactor) {
    $(function () {
        $('.header a').click(function () {
            initJcrop(imageId, jsonId, rawId, scaleFactor); // if other tab
        });
        $('#' + imageId).ready(function () {
            setTimeout('initJcrop("' + imageId + '","' + jsonId + '","' + rawId + '","' + scaleFactor + '")', 100);
            //initJcrop(imageId, jsonId, rawId); // if first tab
        });
    });
}

function initJcrop(imageId, jsonId, rawId, scaleFactor) {

    if ($('#' + imageId).height() > 0) {
        if ($('.img' + imageId + '.ic-crop').size() == 0) {

            // json object
            var json = eval('(' + $('#' + jsonId).val() + ')');

            // store json object to hidden element
            $('#' + jsonId).data('json', json);

            // add current height and width spans
            var coords_id = 'img' + imageId + '_coords';
            $('#' + jsonId).after('<div style="float:left;color:#777;padding: 7px 0 0 0" id="' + coords_id + '"></div>');
            $('#' + coords_id).append('<span class="img' + imageId + ' ic-width"></span>');
            $('#' + coords_id).append('<span class="img' + imageId + ' ic-height"></span>');

            // generate crop links (rel = index)
            for (var i = json.crops.length - 1; i >= 0; i--) {
                var aspectString = '<br/><b>Aspect:</b> ' + (json.crops[i].config.keepAspect ? 'yes' : 'no');
                if (json.crops[i].config.targetWidth * json.crops[i].config.targetHeight == 0) {
                    aspectString = '<br/>&nbsp;';
                }
                $('#' + jsonId).after('<a href="#" class="img' + imageId + ' ic-crop" rel="' + i + '"><b>Name:</b> ' + json.crops[i].name + '<br/><b>Target:</b> ' + json.crops[i].config.targetWidth + 'x' + json.crops[i].config.targetHeight + aspectString + '</a>');
            }
            // create jcrop
            var $cropperFeatures = $('.cropperFeatures');//, $('#' + imageId).parent());
            var cropperFeaturesHeight = $cropperFeatures.height();
            var sF = parseFloat(scaleFactor);
            if (isNaN(sF)) {
                sF = 0.85;
            }
            var opt = {
                boxWidth: (jQuery(window).width() * sF),
                boxHeight: ((jQuery(window).height() - cropperFeaturesHeight) * sF)
            }
            var API = $.Jcrop('#' + imageId,opt);


            icValidateHealth(imageId, jsonId);

            // set current class
            $('.img' + imageId + '.ic-crop').click(function () {
                $('.img' + imageId + '.ic-crop').removeClass('current');
                $(this).addClass('current');
            });

            $('.img' + imageId + '.ic-crop').each(function (i) {
                $(this).click(function () {

                    var json = eval('(' + $('#' + jsonId).val() + ')');
                    var btn = this;
                    var index = this.rel;
                    var crop = json.crops[index];

                    // keep aspect?
                    API.setOptions({
                        aspectRatio: (crop.config.keepAspect ? crop.config.targetWidth / crop.config.targetHeight : 0)
                    });

                    // only change presets
                    API.setOptions({ allowSelect: false });

                    // events
                    API.setOptions({

                        onChange: function (c) {
                            $('.img' + imageId + '.ic-width').html(c.w);
                            $('.img' + imageId + '.ic-height').html('x' + c.h);
                        },

                        onSelect: function (c) {

                            crop.value.x = c.x;
                            crop.value.y = c.y;
                            crop.value.x2 = c.x2;
                            crop.value.y2 = c.y2;
                            json.current = index;

                            $('#' + jsonId).val(JSON.stringify(json));

                            var raw = '';
                            for (var i = 0; i < json.crops.length; i++) {
                                raw += json.crops[i].value.x + ',' +
                                       json.crops[i].value.y + ',' +
                                       json.crops[i].value.x2 + ',' +
                                       json.crops[i].value.y2;
                                if (i < json.crops.length - 1) raw += ';';
                            }
                            $('#' + rawId).val(raw);
                            icValidateHealth(imageId, jsonId);
                        }
                    });

                    var x = crop.value.x;
                    var y = crop.value.y;
                    var x2 = crop.value.x2;
                    var y2 = crop.value.y2;

                    if (x2 == 0) x2 = x + crop.config.targetWidth;
                    if (y2 == 0) y2 = y + crop.config.targetHeight;

                    API.setSelect([x, y, x2, y2]);

                    return false;

                });
            });

            $(".img" + imageId + ".ic-crop[rel='" + json.current + "']").click();

        }
    }
}

function icValidateHealth(imageId, jsonId) {
    var json = eval('(' + $('#' + jsonId).val() + ')');

    $('.img' + imageId + '.ic-crop').each(function (i) {

        var crop = json.crops[i];

        if (crop.value.x2 - crop.value.x < crop.config.targetWidth ||
           crop.value.y2 - crop.value.y < crop.config.targetHeight) {
            $(this).removeClass('healthy');
            $(this).addClass('unhealthy');
        } else {
            $(this).removeClass('unhealthy');
            $(this).addClass('healthy');
        }

    });
}