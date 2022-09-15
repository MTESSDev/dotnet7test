<formulate-form name="form"
                id="formPrincipal"
                v-model="form"
                :invalid-message="invalidMessage"
                :keep-model-data="config.keepData"
                :class="Langue"
                @failed-validation="failedValidation">
    <div v-if="form.EtatRevision === ''">
        <div class="traitement-en-cours md text-center" role="status" style="z-index:1">
            <div class="spinner"></div>
            {{! Le bloc ci-dessous sert à forcer vue a parser le code plutôt que stubble }}
            {{! Stubble lit les '{{' comme des '<%' jusqu'à la fin du bloc }}
            {{=<% %>=}}
            <div v-if="noPageEnCoursRevision > 0" class="mt-16">{{$t("revision.texteAvancement", {0:noPageEnCoursRevision, 1:nbPagesVisibles})}}</div>
                <%={{ }}=%>
            </div>
    </div>
    
    {{# Form.sectionsGroup}}
    <div class="sectionGroup" {{# v-if }} v-if="{{.}}" {{/ v-if}}>
        {{# sections}}
        <div {{# Form.enableVif}}{{# v-if}} v-if="{{.}}" {{/ v-if}}{{/ Form.enableVif}}>
            <div v-if="((!revisionAvecAvancement && form.validAll) || (revisionAvecAvancement && form.validAll && pagesAReviser.includes('{{prefixId}}{{id}}')) || pageCourante.id == '{{prefixId}}{{id}}' && !EstRetourErrorSummary)">
                <div class="section {{classes}}" data-id-page="{{prefixId}}{{id}}"
                     v-show="pageCourante.id == '{{prefixId}}{{id}}'">
                    {{RecursiveComponents components}}
                </div>
            </div>
        </div>
        {{/ sections}}
    </div>
    {{/ Form.sectionsGroup}}

    {{! Le bloc ci-dessous sert à forcer vue a parser les textes des boutons plutôt que stubble }}
    {{! Stubble lit les '{{' comme des '<%' jusqu'à la fin du bloc }}
    {{=<% %>=}}
    <div v-if="pageCourante.id === 'revision'">
        <div :class="[{'d-none' : form.EtatRevision !== 'initial'}]" class="mt-32">
            <button type="button" id="btnValiderFormulaire" @click="validerFormulaire($event)" class="utd-btn primaire">{{ $t("boutons.valider") }}</button>
        </div>
        <div v-if="form.EtatRevision === 'sans-erreur'" class="mt-32" >
            <button type="button" id="btnSoumettre" @click="soumettreFormulaire($event)" class="utd-btn primaire" :disabled="EstModeBacASable">{{ $t("boutons.soumettre") }}</button>
        </div>
    </div>
    <%={{ }}=%>
</formulate-form>