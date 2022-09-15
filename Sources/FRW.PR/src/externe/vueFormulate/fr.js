var VueFormulateI18n = function (e) {
    "use strict";
    function r(e) {
        return "string" == typeof e ? e[0].toUpperCase() + e.substr(1) : e
    }
    //MTESS - Modifier les textes désirés. Copier la variable n et son contenu dans un minifier online ex. https://www.toptal.com/developers/javascript-minifier/  ensuite copier le résultat à l'endroit indiqué dans le fichier .min.js. //TODO pas l'idéal mais ne change pas souvent. À revoir si on a le temps.
    var n = {
        /**
         * Valid accepted value.
         */
        accepted: function ({ name }) {
            return `Merci d'accepter les ${name}.`
        },

        /**
         * The date is not after.
         */
        after: function ({ name, args }) {
            if (Array.isArray(args) && args.length) {
                return `${s(name)} doit être postérieur à ${args[0]}.`
            }
            return `${s(name)} doit être une date ultérieure.`
        },

        /**
         * The value is not a letter.
         */
        alpha: function ({ name }) {
            return `${s(name)} peut uniquement contenir des lettres.`
        },

        /**
         * Rule: checks if the value is alpha numeric
         */
        alphanumeric: function ({ name }) {
            return `${s(name)} peut uniquement contenir des lettres ou des chiffres`
        },

        /**
         * The date is not before.
         */
        before: function ({ name, args }) {
            if (Array.isArray(args) && args.length) {
                return `${s(name)} doit être antérieur à ${args[0]}.`
            }
            return `${s(name)} doit être une date antérieure.`
        },

        /**
         * The value is not between two numbers or lengths
         */
        between: function ({ name, value, args }) {
            const force = Array.isArray(args) && args[2] ? args[2] : false
            if ((!isNaN(value) && force !== 'length') || force === 'value') {
                return `${s(name)} doit être compris entre ${args[0]} et ${args[1]}.`
            }
            return `${s(name)} doit être compris entre ${args[0]} et ${args[1]} caractères.`
        },

        /**
         * The confirmation field does not match
         */
        confirm: function ({ name, args }) {
            return `${s(name)} ne correspond pas.`
        },

        /**
         * Is not a valid date.
         */
        date: function ({ name, args }) {
            if (Array.isArray(args) && args.length) {
                return `${s(name)} n'est pas valide.  Merci d'utiliser le format ${args[0]}`
            }
            return `${s(name)} n'est pas une date valide.`
        },

        /**
         * The default render method for error messages.
         */
        default: function ({ name }) {
            return `Ce champ n'est pas valide.`
        },

        /**
         * Is not a valid email address.
         */
        email: function ({ name, value }) {
            if (!value) {
                return 'Merci d\'entrer une adresse email valide.'
            }
            return `“${value}” n'est pas une adresse email valide.`
        },

        /**
         * Ends with specified value
         */
        endsWith: function ({ name, value }) {
            if (!value) {
                return `Ce champ ne termine pas par une valeur correcte.`
            }
            return `“${value}” ne termine pas par une valeur correcte.`
        },

        /**
         * Value is an allowed value.
         */
        in: function ({ name, value }) {
            if (typeof value === 'string' && value) {
                return `“${s(value)}” n'est pas un(e) ${name} autorisé(e).`
            }
            return `Cette valeur n'est pas un(e) ${name} autorisé(e).`
        },

        /**
         * Value is not a match.
         */
        matches: function ({ name }) {
            return `${s(name)} n'est pas une valeur autorisée.`
        },

        /**
         * The maximum value allowed.
         */
        max: function ({ name, value, args }) {
            if (value.hasOwnProperty('uploadPromise')) {
                return `Le champ « ${name} » ne doit pas contenir plus de ${args[0]} fichier(s).`
            }
            if (Array.isArray(value)) {
                return `Vous pouvez uniquement sélectionner ${args[0]} ${name}.`
            }
            const force = Array.isArray(args) && args[1] ? args[1] : false
            if ((!isNaN(value) && force !== 'length') || force === 'value') {
                return `${s(name)} doit être inférieur ou égal à ${args[0]}.`
            }
            return `${s(name)} doit être inférieur ou égal à ${args[0]} caractères.`
        },

        /**
         * The (field-level) error message for mime errors.
         */
        mime: function ({ name, args }) {
            return `${s(name)} doit être de type: ${args[0] || 'Aucun format autorisé.'}`
        },

        /**
         * The maximum value allowed.
         */
        min: function ({ name, value, args }) {
            if (value.hasOwnProperty('uploadPromise')) {
                return `Le champ « ${name} » doit contenir au moins ${args[0]} fichier(s).`
            }

            if (Array.isArray(value)) {
                return `Vous devez sélectionner au moins ${args[0]} ${name}.`
            }
            const force = Array.isArray(args) && args[1] ? args[1] : false
            if ((!isNaN(value) && force !== 'length') || force === 'value') {
                return `${s(name)} doit être supérieur à ${args[0]}.`
            }
            return `${s(name)} doit être plus long que ${args[0]} caractères.`
        },

        /**
         * The field is not an allowed value
         */
        not: function ({ name, value }) {
            return `“${value}” n'est pas un(e) ${name} autorisé(e).`
        },

        /**
         * The field is not a number
         */
        number: function ({ name }) {
            return `${s(name)} doit être un nombre.`
        },

        /**
         * Required field.
         */
        required: function ({ name }) {
            return `${s(name)} est obligatoire.`
        },

        /**
         * Starts with specified value
         */
        startsWith: function ({ name, value }) {
            if (!value) {
                return `Ce champ ne commence pas par une valeur correcte.`
            }
            return `“${value}” ne commence pas par une valeur correcte.`
        },

        /**
         * Value is not a url.
         */
        url: function ({ name }) {
            return `Merci d'entrer une URL valide.`
        }
    };
    return e.fr = function (e) { var r; e.extend({ locales: (r = {}, r.fr = n, r) }) }, e
}({});