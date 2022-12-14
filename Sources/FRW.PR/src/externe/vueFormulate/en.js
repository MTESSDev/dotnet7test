var VueFormulateI18nEn = function (e) {
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
            return `Please accept the ${name}.`
        },

        /**
         * The date is not after.
         */
        after: function ({ name, args }) {
            if (Array.isArray(args) && args.length) {
                return `${s(name)} must be after ${args[0]}.`
            }
            return `${s(name)} must be a later date.`
        },

        /**
         * The value is not a letter.
         */
        alpha: function ({ name }) {
            return `${s(name)} can only contain alphabetical characters.`
        },

        /**
         * Rule: checks if the value is alpha numeric
         */
        alphanumeric: function ({ name }) {
            return `${s(name)} can only contain letters and numbers.`
        },

        /**
         * The date is not before.
         */
        before: function ({ name, args }) {
            if (Array.isArray(args) && args.length) {
                return `${s(name)} must be before ${args[0]}.`
            }
            return `${s(name)} must be an earlier date.`
        },

        /**
         * The value is not between two numbers or lengths
         */
        between: function ({ name, value, args }) {
            const force = Array.isArray(args) && args[2] ? args[2] : false
            if ((!isNaN(value) && force !== 'length') || force === 'value') {
                return `${s(name)} must be between ${args[0]} and ${args[1]}.`
            }
            return `${s(name)} must be between ${args[0]} and ${args[1]} characters long.`
        },

        /**
         * The confirmation field does not match
         */
        confirm: function ({ name, args }) {
            return `${s(name)} does not match.`
        },

        /**
         * Is not a valid date.
         */
        date: function ({ name, args }) {
            if (Array.isArray(args) && args.length) {
                return `${s(name)} is not a valid date, please use the format ${args[0]}`
            }
            return `${s(name)} is not a valid date.`
        },

        /**
         * The default render method for error messages.
         */
        default: function ({ name }) {
            return `This field isn’t valid.`
        },

        /**
         * Is not a valid email address.
         */
        email: function ({ name, value }) {
            if (!value) {
                return 'Please enter a valid email address.'
            }
            return `“${value}” is not a valid email address.`
        },

        /**
         * Ends with specified value
         */
        endsWith: function ({ name, value }) {
            if (!value) {
                return `This field doesn’t end with a valid value.`
            }
            return `“${value}” doesn’t end with a valid value.`
        },

        /**
         * Value is an allowed value.
         */
        in: function ({ name, value }) {
            if (typeof value === 'string' && value) {
                return `“${s(value)}” is not an allowed ${name}.`
            }
            return `This is not an allowed ${name}.`
        },

        /**
         * Value is not a match.
         */
        matches: function ({ name }) {
            return `${s(name)} is not an allowed value.`
        },

        /**
         * The maximum value allowed.
         */
        max: function ({ name, value, args }) {
            if (value.hasOwnProperty('uploadPromise')) {
                return `The "${name}" can only contain ${args[0]} file(s).`
            }
            if (Array.isArray(value)) {
                return `You may only select ${args[0]} ${name}.`
            }
            const force = Array.isArray(args) && args[1] ? args[1] : false
            if ((!isNaN(value) && force !== 'length') || force === 'value') {
                return `${s(name)} must be less than or equal to ${args[0]}.`
            }
            return `${s(name)} must be less than or equal to ${args[0]} characters long.`
        },

        /**
         * The (field-level) error message for mime errors.
         */
        mime: function ({ name, args }) {
            return `${s(name)} must be of the type: ${args[0] || 'No file formats allowed.'}`
        },

        /**
         * The maximum value allowed.
         */
        min: function ({ name, value, args }) {
            if (value.hasOwnProperty('uploadPromise')) {
                return `The "${name}" field need at least ${args[0]} file(s).`
            }

            if (Array.isArray(value)) {
                return `You need at least ${args[0]} ${name}.`
            }
            const force = Array.isArray(args) && args[1] ? args[1] : false
            if ((!isNaN(value) && force !== 'length') || force === 'value') {
                return `${s(name)} must be at least ${args[0]}.`
            }
            return `${s(name)} must be at least ${args[0]} characters long.`
        },

        /**
         * The field is not an allowed value
         */
        not: function ({ name, value }) {
            return `“${value}” is not an allowed ${name}.`
        },

        /**
         * The field is not a number
         */
        number: function ({ name }) {
            return `${s(name)} must be a number.`
        },

        /**
         * Required field.
         */
        required: function ({ name }) {
            return `${s(name)} is required.`
        },

        /**
         * Starts with specified value
         */
        startsWith: function ({ name, value }) {
            if (!value) {
                return `This field doesn’t start with a valid value.`
            }
            return `“${value}” doesn’t start with a valid value.`
        },

        /**
         * Value is not a url.
         */
        url: function ({ name }) {
            return `Please include a valid url.`
        }
    };
    return e.en = function (e) { var r; e.extend({ locales: (r = {}, r.en = n, r) }) }, e
}({});