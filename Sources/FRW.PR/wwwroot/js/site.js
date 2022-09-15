//Traitement exécuté dès que le fichier JS est chargé.
definirParametresDefautSelect2();

//Fonction interne requise pour l'override de messages de validations avec nos fichiers i18n
function s(item) {
    if (typeof item === 'string') {
        return item[0].toUpperCase() + item.substr(1)
    }
    return item
}


function obtenirTexteEdite(id) {
    return '(Patch) Non défini.'
}

/* ====================== Fonctions communes de validations ======================*/
function estNAS(n) { if (typeof n == 'number' && n >= 0 && n < 1000000000 && n % 1 == 0) n = ('0'.repeat(8) + String(n)).slice(-9); if (!/^[0-9]{3}[ -]?[0-9]{3}[ -]?[0-9]{3}$/.test(n = String(n))) return false; n = n.replace(/[^0-9]/g, ''); return [].map.call(n, function (v, k) { var m = (k % 2) + 1, v = parseInt(v) * m; if (v >= 10) return 1 + (v % 10); return v; }).reduce(function (a, b) { return a + b; }, 0) % 10 == 0; }

function estTelephone(valeur, estOnzeChiffresPermis) {
    //On enlève les espaces, tirets et parenthèses
    const nombresEtTexte = valeur.replace(/(\s|-|\(|\))/g, '')
    const regex = estOnzeChiffresPermis ? new RegExp(/^\d{10,11}$/) : new RegExp(/^\d{10}$/)
    return regex.test(nombresEtTexte)
}

/* ===============================================================================*/


/* ====================== Fonctions communes pour formattage ======================*/
function obtenirValeurChampAFormater(event) {
    if (event && event.currentTarget && event.currentTarget.value) {
        return event.currentTarget.value
    }
    return ''
}
function appliquerFormatageChamp(event, valeurEpuree, masque, forcerMajuscules) {

    if (event && event.currentTarget && event.currentTarget.value) {
        let valeurFormatee = ''

        for (var i = 0; i < valeurEpuree.length; i++) {
            valeurFormatee += (masque[i] || '') + valeurEpuree[i]
        }

        valeurFormatee = forcerMajuscules ? valeurFormatee.toUpperCase() : valeurFormatee

        if (event.currentTarget.value != valeurFormatee) {
            event.currentTarget.value = valeurFormatee

            event.stopPropagation()
            event.preventDefault()

            //Nécessaire afin que vue sache que la valeur a changé et mette à jour le modèle (sinon la valeur non formatée revient)
            event.currentTarget.dispatchEvent(new Event('change'))
        }
    }
}
function obtenirNombre(valeur, nbDecimales) {
    const nombres = Number(valeur).toString().split('.')
    if (nbDecimales === 0 && !nombres[1]) {
        return nombres[0]
    }
    
    let decimales = nombres[1] || '0'
    decimales = decimales.padEnd(nbDecimales, '0')
    return nombres[0] + '.' + decimales
}
function obtenirDateLocale(paramDate, inclureHeure) {    
    if (!paramDate || paramDate === '' || paramDate === undefined) {
        paramDate = Date.now()
    }
    var date = new Date(paramDate)
    var d = new Date()

    d.setUTCFullYear(date.getUTCFullYear())
    d.setUTCMonth(date.getUTCMonth())
    d.setUTCDate(date.getUTCDate())
    if (inclureHeure) {
        d.setUTCHours(date.getUTCHours())
        d.setUTCMinutes(date.getUTCMinutes())
        d.setUTCSeconds(date.getUTCSeconds())
        d.setUTCMilliseconds(date.getUTCMilliseconds())
    }
    else {
        d.setUTCHours(0)
        d.setUTCMinutes(0)
        d.setUTCSeconds(0)
        d.setUTCMilliseconds(0)
    }
    return d
}
/* ===============================================================================*/


function obtenirValeurParametreUrl(parametre) {
    parametre = parametre.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    const expr = "[\\?&]" + parametre + "=([^&#]*)";
    const regex = new RegExp(expr);
    const results = regex.exec(window.location.search);
    if (results !== null) {
        return results[1];
    } else {
        return false;
    }
}

function definirLocalStorageAvecExpiration(prefix, cle, valeur, ttl) {
    const now = new Date()

    const item = {
        value: valeur,
        expiry: now.getTime() + ttl,
    }
    localStorage.setItem(prefix + cle, JSON.stringify(item))
}

function obtenirLocalStorageAvecExpiration(prefix, cle) {
    const itemStr = localStorage.getItem(prefix + cle)

    if (!itemStr) {
        return null
    }
    const item = JSON.parse(itemStr)
    const now = new Date()

    if (now.getTime() > item.expiry) {
        localStorage.removeItem(prefix + cle)
        return null
    }
    return item.value
}

function nettoyerLocalStorageAvecExpiration(prefix) {
    for (var i = 0; i < localStorage.length; i++) {
        var key = localStorage.key(i)

        if (key.startsWith(prefix)) {
            obtenirLocalStorageAvecExpiration('', localStorage.key(i))
        }
    }
}


function definirParametresDefautSelect2() {
    if ($.fn.select2 != undefined) {
        $.fn.select2.defaults.set("theme", "bootstrap4");
        $.fn.select2.defaults.set("language", $('html').attr('lang') || 'fr');
    }
};

function ajusterAccessibiliteSelect2Multiple(controlesSelect2) {

    //Bien que l'initialisation d'un champ avec select2 soit synchrone, pour une raison obscure il semble que le tabindex du span de sélection est remis à -1 après l'initialisation... Le setTimeout nous garantie que notre traitement va passer au bon moment... Peut-être à revoir éventuellement.
    setTimeout(function () {
        $(controlesSelect2).each(function () {
            var conteneurControle = $(this).parent();
            var spanSelection = conteneurControle.find(".select2-selection.select2-selection--multiple");

            //Permettre focus sur span de sélection et aussi lui ajouter un libellé  (afin d'avoir le nom du champ au lecteur écran au focus). Cache le libellé au lecteur écran (ne pouvait pas être lié au champ, ce qu'on vient de corriger avec aria-label, donc était une information en double au lecteur écran à la lecture séquentielle du formulaire.)
            var libelle = spanSelection.closest(".select2-container").prevAll("label:first");
            spanSelection.attr("aria-label", libelle.text()).attr("tabindex", "0");
            libelle.attr("aria-hidden", "true");

            //TODO trouver un moyen pour donner le focus au span "combobox" qui est focusable lors du click sur le libellé, mais ça ne semble pas fonctionner.
            //libelle.on("mousedown", function () {
            //    selection.focus();
            //});

            //On désactive la saisie manuelle
            conteneurControle.find('.select2-search__field').prop('disabled', true);

            //Pills des éléments sélectionnés. À chaque sélection, les pills sont reconstruites. On doit masquer le X au lecteur écran.
            $(this).on('select2:select select2:unselect', function (e) {
                var conteneurControle = $(this).parent();
                conteneurControle.find(".select2-selection__choice__remove").attr("aria-hidden", "true");
            });
        });
    }, 100);
};

/**
 * Obtient le offsetTop d'un élément par rapport à la page en entier (sinon normalement c'est par rapport à son conteneur parent).
 * @param {Object} element Élément pour lequel on doit obtenir le offsetTop.
 */
function obtenirOffsetTop (element) {
    let offsetTop = 0;
    while (element) {
        offsetTop += element.offsetTop;
        element = element.offsetParent;
    }
    return offsetTop;
}