// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function LoadResults() {
    console.log('loading results');

    //$(".active").parents('.error-type-container').css("height", "-webkit-fill-available");

    $('.keywords-collapsible, .errors-collapsible').click(function () {
        var parent = $(this).parent()[0];
        if ($(this).hasClass('active')) {
            $(this).removeClass('active');
            parent.style['height'] = '180px';
        } else {
            $(this).addClass('active');
            parent.style['height'] = 'unset';
        }
    });

    $('.error-container').hover(function () {
        var id = $(this).attr('wordId');
        if (id !== undefined) {
            var selector = ".word-error[wordId='" + id + "']";
            $(selector).toggleClass('word-selected');
        }
        var refnum = $(this).attr('refnum');
        if (refnum !== undefined) {
            var selector = ".ref-error[number='" + refnum + "']";
            $(selector).toggleClass('ref-selected');
        }
        var sectId = $(this).attr('sectId');
        if (sectId !== undefined) {
            var selector = ".section-error[sectId='" + sectId + "']";
            $(selector).toggleClass('section-selected');
        }
    });

    $('.error-container').blur(function () {
        var id = $(this).attr('wordId');
        if (id !== undefined) {
            var selector = ".word-error[wordId='" + id + "']";
            $(selector).removeClass('word-selected');
        }
        var refnum = $(this).attr('refnum');
        if (refnum !== undefined) {
            var selector = ".ref-error[number='" + refnum + "']";
            $(selector).removeClass('ref-selected');
        }
        var sectId = $(this).attr('sectId');
        if (sectId !== undefined) {
            var selector = ".section-error[sectId='" + sectId + "']";
            $(selector).removeClass('section-selected');
        }
    });

    $('.word-error').hover(function () {
        var errors = $(this).attr('errors');
        if (errors !== undefined) {
            var errorCodes = errors.split(',');
            var errorCodesSelector = (errorCodes.map(x => "[errorcode='" + x + "']")).join(',');
            $(errorCodesSelector).toggleClass('error-highlighted');
        }
        var id = $(this).attr('wordId');
        var selector = ".error-container[wordId='" + id + "']";
        $(selector).toggleClass('error-highlighted');
    })

    $('.word-error').blur(function () {
        var errors = $(this).attr('errors');
        if (errors !== undefined) {
            var errorCodes = errors.split(',');
            var errorCodesSelector = (errorCodes.map(x => "[errorcode='" + x + "']")).join(',');
            $(errorCodesSelector).removeClass('error-highlighted');
        }
        var id = $(this).attr('wordId');
        var selector = ".error-container[wordId='" + id + "']";
        $(selector).removeClass('error-highlighted');
    })

    $('.ref-error').hover(function () {
        var errors = $(this).attr('errors');
        if (errors !== undefined) {
            var errorCodes = errors.split(',');
            var errorCodesSelector = (errorCodes.map(x => "[errorcode='" + x + "']")).join(',');
            $(errorCodesSelector).toggleClass('error-highlighted');
        }
        var refnum = $(this).attr('number');
        var selector = ".error-container[refnum='" + refnum + "']";
        $(selector).toggleClass('error-highlighted');
    })

    $('.ref-error').blur(function () {
        var errors = $(this).attr('errors');
        if (errors !== undefined) {
            var errorCodes = errors.split(',');
            var errorCodesSelector = (errorCodes.map(x => "[errorcode='" + x + "']")).join(',');
            $(errorCodesSelector).removeClass('error-highlighted');
        }
        var refnum = $(this).attr('number');
        if (refnum !== undefined) {
            var selector = ".error-container[refnum='" + refnum + "']";
            $(selector).removeClass('error-highlighted');
        }
    })

    $('.section-error').hover(function () {
        var errors = $(this).attr('errors');
        if (errors !== undefined) {
            var errorCodes = errors.split(',');
            var errorCodesSelector = (errorCodes.map(x => "[errorcode='" + x + "']")).join(',');
            $(errorCodesSelector).toggleClass('error-highlighted');
        }
        var sectId = $(this).attr('sectId');
        if (sectId !== undefined) {
            var selector = ".error-container[sectId='" + sectId + "']";
            $(selector).toggleClass('error-highlighted');
        }
    })

    $('.section-error').blur(function () {
        var errors = $(this).attr('errors');
        if (errors !== undefined) {
            var errorCodes = errors.split(',');
            var errorCodesSelector = (errorCodes.map(x => "[errorcode='" + x + "']")).join(',');
            $(errorCodesSelector).removeClass('error-highlighted');
        }
        var sectId = $(this).attr('sectId');
        if (sectId !== undefined) {
            var selector = ".error-container[sectId='" + sectId + "']";
            $(selector).removeClass('error-highlighted');
        }
    })

    // TODO: refactor the code above, too much repeating code

    $('.keyword-container, a.papername-word').click(function () {
        var currentStatus = $(this).hasClass('toggled-on')
        highlightKeyword.bind(this)(!currentStatus)
        $(this).toggleClass('word-selected toggled-on', !currentStatus)
    })

    $('.keyword-container, a.papername-word').hover(function () {
        if (!$(this).hasClass('toggled-on')) {
            highlightKeyword.bind(this)(true)
            $(this).toggleClass('word-selected', true)
        }
    }, function () {
        if (!$(this).hasClass('toggled-on')) {
            highlightKeyword.bind(this)(false)
            $(this).toggleClass('word-selected', false)
        }
    })

    function highlightKeyword(state) {
        var ids = $(this).attr('wordIds');
        if (ids !== undefined) {
            var ids = ids.split(',')
            var selector = ids.map(id => `[wordId='${id}']`).join(',')
            $(selector).each(function () {
                let dir = state == true ? 1 : -1;
                let sourceCount = $(this).attr('source-count');
                sourceCount = sourceCount ? parseInt(sourceCount) : 0;
                sourceCount += dir;
                $(this).attr('source-count', sourceCount)
                $(this).toggleClass('word-selected', (sourceCount > 0))
            })
        }
    }

    $('.error-container').click(function () {
        var currentStatus = $(this).hasClass('toggled-on')
        highlightKeyword.bind(this)(!currentStatus)
        $(this).toggleClass('word-selected toggled-on', !currentStatus)
    })

    $('.error-container').hover(function () {
        if (!$(this).hasClass('toggled-on')) {
            highlightSentence.bind(this)(true)
            $(this).toggleClass('word-selected', true)
        }
    }, function () {
        if (!$(this).hasClass('toggled-on')) {
            highlightSentence.bind(this)(false)
            $(this).toggleClass('word-selected', false)
        }
    })

    function highlightSentence(state) {
        var ids = $(this).attr('sentId');
        if (ids !== undefined) {
            var ids = ids.split(',')
            var selector = ids.map(id => `[sentId='${id}']`).join(',')
            $(selector).each(function () {
                let dir = state == true ? 1 : -1;
                let sourceCount = $(this).attr('source-count');
                sourceCount = sourceCount ? parseInt(sourceCount) : 0;
                sourceCount += dir;
                $(this).attr('source-count', sourceCount)
                $(this).toggleClass('word-selected', (sourceCount > 0))
            })
        }
    }
}