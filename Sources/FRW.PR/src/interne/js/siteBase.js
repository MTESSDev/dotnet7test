/*! siteBase */
/* Traitements de base disponibles dans toutes les pages du site.
*/

//Traitement exécuté dès que le fichier JS est chargé.

definirTrappeErreurGlboale();


//Traitement exécuté lorsque le rendu de l'interface est complété.
$(document).ready(function () {

    ajusterAccessibiliteLiens();

    $(".espaceur-conteneurs:visible:last").addClass("dernier-visible");
});

function ajusterAccessibiliteLiens() {
    $("main").find('a[target="_blank"]').each(function () {
        var lien = $(this);

        //Ne pas traiter les liens sans href et les liens
        if (!lien.attr('href')) {
            return;
        }
        //Ne pas traiter les liens vers des fichiers ou les liens ayant explicitement une classe indiquant de ne pas les considérer comme lien externe(sans-lien-externe).  TODO éventuellement rafiner la condition afin de traiter uniquement les liens vers des pages web...
        if (lien.attr('href').endsWith(".pdf") || lien.hasClass("sans-lien-externe")) {
            return;
        }

        //Ajouter la classe lien-externe afin d'afficher l'icône d'ouverture dans une nouvelle fenêtre.
        if (!lien.hasClass("lien-externe") && !lien.find(".lien-externe").length) {
            lien.addClass("lien-externe");
        }

        //Ajouter le texte pour accessibilité nouvelle fenêtre si on détecte qu'il n'est pas là. TODO rafiner cette vérification éventuellement au besoin, il pourrait arriver qu'un .sr-only soit là pour autre chose dans le lien (très peu probable)
        if (lien.find(".sr-only").length === 0) {
            var texteAccessibilite = obtenirTexteEdite("txtcacheCommunOuvertureLienNouvelOnglet");
            lien.append('<span class="sr-only">{0}</span>'.format(texteAccessibilite));
        }
    });

};

/**
* Définit un traitement de gestion globale des erreurs de script pour l'application. (Trappe d'erreur globale pour toute l'application).
*/
function definirTrappeErreurGlboale() {

    window.onerror = function (msg, url, lineNo, columnNo, error) {
        var detailsTechniques = "";

        //Déterminer les détails techniques selon la disponibilité du paramètre error (non disponible dans tous les fureteurs)
        if (error) {
            var stack = error.stack;
            detailsTechniques = error.toString();

            if (stack) {
                detailsTechniques += `\n Stack : ${stack}`;
            }
        }
        else {
            try {
                const stack = new Error().stack;
                detailsTechniques = `Message : ${msg} \nLigne : ${lineNo} \nColonne : ${columnNo} \nStack : ${stack}`;
            } catch(e) {
                detailsTechniques = `Message : ${msg} \nLigne : ${lineNo} \nColonne : ${columnNo}`;
            }
        }

        journaliserErreurJs(detailsTechniques);

        return false;
    };

    window.addEventListener('unhandledrejection', function (event) {
        journaliserErreurJs(event.reason.stack);
    });
};

function journaliserErreurJs(message) {
    executerRequeteAjax('/api/log/error', { url: window.location.href, message: JSON.stringify(message, maskInfo, 2) });
};

function journaliserInformationJs(message) {
    executerRequeteAjax('/api/log/information', { url: window.location.href, message: JSON.stringify(message) });
};

function maskInfo(key, value) {
    var maskedValue = value;
    if (key.includes("data")) {
        var data = ""
        try { data= JSON.parse(value) } catch (e) { }
        maskedValue = JSON.stringify(data, maskData, 2)
    }
    return maskedValue;
}

function maskData(key, value) {
    var maskedValue = value;
    if (key.includes("courriel")) {
        maskedValue = "*****";
    }
    if (key.includes("Nas")) {
        maskedValue = "*****";
    }
    if (key.includes("nas")) {
        maskedValue = "*****";
    }
    if (key.includes("NumeroAssuranceSociale")) {
        maskedValue = "*****";
    }
    if (key.includes("Nam")) {
        maskedValue = "*****";
    }
    if (key.includes("nam")) {
        maskedValue = "*****";
    }
    if (key.includes("NumeroAssuranceMaladie")) {
        maskedValue = "*****";
    }
    if (key == "motPasse") {
        maskedValue = "*****";
    }
    return maskedValue;
}

//TODO faire un wrapper js pour les posts de VC
function executerRequeteAjaxBase(url, donnees) {

    donnees = donnees || {};

    return $.ajax({
        type: "POST",
        url: url,
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",
                $('input:hidden[name="__RequestVerificationToken"]:first').val());
        },
        data: donnees
    });
};

function executerRequeteAjaxBaseGet(url, donnees, cache) {

    donnees = donnees || {};
    cache = cache !== false;

    return $.ajax({
        type: "GET",
        url: url,
        beforeSend: function (xhr) {
            if (!cache) {
                xhr.setRequestHeader("cache-control", "no-cache");
            } else {
                xhr.setRequestHeader("session_id", SESSION_ID);
            }
        },
        data: donnees
    });
};

function executerRequeteAjaxGet(url, donnees, conteneur, cache) {
    return executerRequeteAjax(url, donnees, conteneur, 'GET', cache)
};

function executerRequeteAjax(url, donnees, conteneur, methode, cache) {

    methode = methode || 'POST';
    cache = cache !== false;

    var dfd = $.Deferred(function () {

        donnees = donnees || {};

        if (conteneur && conteneur.find(".traitement-en-cours").length == 0) {
            afficherChargementEnCours(conteneur);

            //Si la requête ajax été initiée par un bouton on donne le focus au contrôle de chargement en cours (accessibilité).
            if (document.activeElement) {
                if ($(document.activeElement).is("button, input[type=submit]")) {
                    var spinner = conteneur.find(".traitement-en-cours");
                    spinner.focus();
                    spinner.get(0).scrollIntoView();
                    window.scrollBy(0, -48);
                }
            }
        }

        var executionRequeteAjax;
        switch (methode) {
            case "POST":
                executionRequeteAjax = executerRequeteAjaxBase;
                break;
            case "GET":
                executionRequeteAjax = executerRequeteAjaxBaseGet;
                break;
            default: alert('Méthode ' + methode + ' non gérée.');
                break;
        }

        executionRequeteAjax(url, donnees, cache)
            .done(function (retour) {
                if (retour && retour.resultat && retour.resultat === 'redirection' && retour.url) {
                    window.location.href = retour.url;
                    dfd.resolve(retour);
                } else {
                    if (conteneur) {

                        //Si le contrôle de traitement en cours a actuellement le focus, on donne le focus au premier élément focusable du conteneur. Si aucun élément focusable on tente de donner le focus au conteneur lui même. (accessibilité liée au chargement en cours).
                        var donnerFocusConteneur = document.activeElement && $(document.activeElement).is(".traitement-en-cours");

                        conteneur.fadeTo(50, 0.2, function () {
                            conteneur.html(retour);
                            dfd.resolve(retour);

                            conteneur.fadeTo(350, 1, function () {
                                if (donnerFocusConteneur) {
                                    var elementsFocusables = obtenirElementsFocusables(conteneur.get(0));
                                    if (elementsFocusables.length > 0) {
                                        elementsFocusables[0].focus();
                                    }
                                    else {
                                        conteneur.focus();
                                    }
                                }
                            });
                        });
                    } else {
                        dfd.resolve(retour);
                    }
                }
            })
            .fail(function (erreur) {
                //TODO Enlever l'appel test à afficherMessageErreur
                //afficherMessageErreurTest(erreur);

                if (conteneur) {
                    //Si le contrôle de traitement en cours a actuellement le focus, on donne le focus au premier élément focusable du conteneur. Si aucun élément focusable on tente de donner le focus au conteneur lui même. (accessibilité liée au chargement en cours).
                    var donnerFocusConteneur = document.activeElement && $(document.activeElement).is(".traitement-en-cours");

                    conteneur.fadeTo(50, 0.2, function () {
                        conteneur.html('<div class="info-non-disponible d-flex flex-column align-items-center" tabindex="0"><div aria-hidden="true" class="utd-icone-svg avertissement lg"></div><div class="texte utd-text-base">{0}</div></div>'.format(obtenirTexteEdite("messageCommunInformationNonDisponible")));
                        dfd.reject(erreur);

                        conteneur.fadeTo(350, 1, function () {
                            if (donnerFocusConteneur) {
                                var elementsFocusables = obtenirElementsFocusables(conteneur.get(0));
                                if (elementsFocusables.length > 0) {
                                    elementsFocusables[0].focus();
                                } else {
                                    conteneur.focus();
                                }
                            }
                        });
                    });
                } else {
                    dfd.reject(erreur);
                }
            });
    });

    return dfd.promise();
};

function afficherChargementEnCours(conteneur, parametres) {

    var valeursDefaut = {
        taille: "md",
        position: "text-center",
        afficherTexte: false,
        texte: obtenirTexteEdite('texteCommunChargementEnCours'),
        overlay: false,
        affichageDansBouton: false //non utilisé pour le moment
    };

    parametres = $.extend({}, valeursDefaut, parametres);

    var texte = '<div class="texte sr-only">{0}</div>'.format(parametres.texte);
    texte = parametres.afficherTexte ? texte.replace("sr-only", "") : texte;

    var classeOverlay = parametres.overlay ? 'avec-overlay' : '';

    var html = '<div class="traitement-en-cours {0} {1} {2}" tabindex="0" role="status">' +
        '<div class="spinner"></div>' +
        '{3}</div>';
    html = html.format(classeOverlay, parametres.taille, parametres.position, texte);

    var controle = $(html);

    if (conteneur.height() > 0) {
        controle.width(conteneur.width());
        controle.height(conteneur.height());
    } else {
        controle.css("position", "relative");
    }

    conteneur.html(controle);
    controle.css("opacity", 1);
};

//====================================================================
//format
//
//Permet de modifier de façon dynamique le contenu d'une chaîne de caractères, exactement comme le fait string.format() en c#.
//
//Retourne : La chaîne de caractère formatée.
//
//Exemple(s) d'utilisation :
// var test = "Un petit texte avec insertion dynamique. Valeur dynamique1 = {0}. Valeur dynamique2 = {1}".format(maVariableContenantValeur, maVariableContenantValeur2);
//====================================================================
// Vérifier si la fontion est déjà implémentée, si ce n'est pas le cas on l'implémente.
if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
                ? args[number]
                : match
                ;
        });
    };
}

/**
 * Permet de modifier une URL relative vers une page interne de l'espace client pour qu'elle respecte la langue d'affichage.
 * @param {string} url URL relative vers une page interne de l'espace client.
 * @returns {string} URL multilingue.
 * */
function definirLienMultilingue(url) {
    var urlMultilingue = url;

    // Pas besoin de modifier l'URL si on est français car c'est la langue par défaut
    var langue = $('html').attr('lang');
    if (langue != 'fr') {

        // S'assurer que l'URL respecte le format attendu
        if (!urlMultilingue.startsWith('/')) {
            urlMultilingue = '/' + urlMultilingue;
        }

        // S'assurer que l'URL commence par la langue d'affichage
        if (!urlMultilingue.toLowerCase().startsWith('/' + langue)) {
            urlMultilingue = '/' + langue + '/' + urlMultilingue.substring(1);
        }
    }

    return urlMultilingue;
};

/* ---- TODO DÉBUT - Bloc à vérifier si peut être déplacé dans Async ---- */

/**
 * Obtient les éléments focusables à l'intérieur de l'élément spécifié.
 * @param {object} element Objet javascript pour lequel on veut les éléments focusables.
 */
function obtenirElementsFocusables(element) {
    //TODO décider si on doit considérer un élément qui n'est pas un contrôle comme un élément focusable. Peut-être ajouter un param pour ca?
    //[tabindex]: not([tabindex = "-1"])
    return element.querySelectorAll('a[href]:not([disabled]), button:not([disabled]), textarea:not([disabled]), input:not([type="hidden"]):not([disabled]), select:not([disabled])')
};

/**
 * Obtient les éléments focusables à l'intérieur du formulaire spécifié. Est plus complète que obtenirElementsFocusables qui n'exclut pas les éléments non visibles et elle retourne uniquement sur les éléments permettant la saisie ou une action (boutons)
 * Attention! Cette fonction retourne les objets jQuery et non javascript comme dans obtenirElementsFocusables.
 * @param {object} $element Objet jQuery pour lequel on veut les éléments focusables.
 */
function obtenirElementsFocusablesFormulaire($element) {
    return $element.find('input, select, textarea, button').not('[disabled], :hidden')
};

//====================================================================
//Génère un id unique.
//
//Retourne : L'id unique généré
//
//====================================================================
function genererId() {
    return Date.now().toString(36) + '-' + Math.random().toString(36).substr(2, 9);
};