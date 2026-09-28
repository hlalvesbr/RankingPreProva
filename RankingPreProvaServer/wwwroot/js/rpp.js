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
