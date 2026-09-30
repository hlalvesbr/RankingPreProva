// Efeitos visuais e integração com anúncios do RankingPreProva.
window.rppFx = {
    pronto: function () {
        const splash = document.getElementById('app-splash');
        if (!splash) return;
        splash.classList.add('saindo');
        setTimeout(() => splash.remove(), 400);
    },
    confete: function () {
        if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
        const cores = ['#1f5f6b', '#d9a425', '#3fa3a8', '#b23b3b', '#2f7d4a', '#6a3d8a'];
        for (let i = 0; i < 90; i++) {
            const c = document.createElement('div');
            c.className = 'confete';
            c.style.left = Math.random() * 100 + 'vw';
            c.style.background = cores[i % cores.length];
            c.style.animationDuration = (1.8 + Math.random() * 1.8) + 's';
            c.style.animationDelay = (Math.random() * 0.4) + 's';
            c.style.transform = 'rotate(' + Math.random() * 360 + 'deg)';
            c.style.borderRadius = Math.random() > .5 ? '50%' : '2px';
            document.body.appendChild(c);
            setTimeout(() => c.remove(), 4200);
        }
    },
    topo: function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    },
    confirmarSaida: function (ativo) {
        window.onbeforeunload = ativo ? () => true : null;
    },
    compartilhar: async function (titulo, texto, url) {
        if (navigator.share) {
            try {
                await navigator.share({ title: titulo, text: texto, url: url });
                return 'compartilhado';
            } catch (e) {
                if (e && e.name === 'AbortError') return 'cancelado';
            }
        }
        try {
            await navigator.clipboard.writeText(url);
            return 'copiado';
        } catch (e) {
            return 'erro';
        }
    }
};

window.rppMd = {
    envolver: function (el, antes, depois, exemplo) {
        const ini = el.selectionStart, fim = el.selectionEnd, v = el.value;
        const sel = v.substring(ini, fim) || exemplo;
        el.value = v.substring(0, ini) + antes + sel + depois + v.substring(fim);
        el.focus();
        el.setSelectionRange(ini + antes.length, ini + antes.length + sel.length);
        return el.value;
    },
    prefixar: function (el, prefixo) {
        const v = el.value;
        const ini = v.lastIndexOf('\n', el.selectionStart - 1) + 1;
        let fim = v.indexOf('\n', Math.max(el.selectionEnd - (el.selectionEnd > el.selectionStart ? 1 : 0), ini));
        if (fim < 0) fim = v.length;
        const linhas = v.substring(ini, fim).split('\n');
        const novo = linhas.map((l, i) => (prefixo === '1. ' ? (i + 1) + '. ' : prefixo) + l).join('\n');
        el.value = v.substring(0, ini) + novo + v.substring(fim);
        el.focus();
        el.setSelectionRange(ini, ini + novo.length);
        return el.value;
    }
};

window.rppAds = {
    config: function (posicao) {
        const ler = (nome) => document.querySelector('meta[name="' + nome + '"]')?.content || '';
        return { clientId: ler('adsense-client'), slot: ler('adsense-slot-' + posicao) };
    },
    carregar: function () {
        try {
            (window.adsbygoogle = window.adsbygoogle || []).push({});
        } catch (e) {
            console.debug('AdSense indisponível', e);
        }
    }
};
