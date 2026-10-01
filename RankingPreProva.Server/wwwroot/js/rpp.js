// Efeitos visuais do RankingPreProva.
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
    },
    inserir: function (el, texto) {
        const ini = el.selectionStart, fim = el.selectionEnd, v = el.value;
        const antes = ini > 0 && v[ini - 1] !== '\n' ? '\n' : '';
        const depois = fim < v.length && v[fim] !== '\n' ? '\n' : '';
        const bloco = antes + texto + depois;
        el.value = v.substring(0, ini) + bloco + v.substring(fim);
        el.focus();
        el.setSelectionRange(ini + bloco.length, ini + bloco.length);
        return el.value;
    },
    registrarImagens: function (container, dotnet) {
        const tipos = ['image/png', 'image/jpeg', 'image/gif', 'image/webp'];
        const enviar = async (arquivos) => {
            for (const f of arquivos) {
                if (!f.type.startsWith('image/')) continue;
                if (!tipos.includes(f.type)) { await dotnet.invokeMethodAsync('ImagemRejeitada', 'Formato não suportado. Use PNG, JPG, GIF ou WebP.'); continue; }
                if (f.size > 1024 * 1024) { await dotnet.invokeMethodAsync('ImagemRejeitada', 'A imagem deve ter no máximo 1 MB.'); continue; }
                const bytes = new Uint8Array(await f.arrayBuffer());
                await dotnet.invokeMethodAsync('ReceberImagem', bytes, f.type);
            }
        };
        const imagensDe = (lista) => Array.from(lista || []).filter(f => f.type.startsWith('image/'));
        const aoColar = (e) => {
            if (e.target.tagName !== 'TEXTAREA') return;
            const arquivos = imagensDe(e.clipboardData && e.clipboardData.files);
            if (!arquivos.length) return;
            e.preventDefault();
            enviar(arquivos);
        };
        const aoArrastarSobre = (e) => {
            if (e.dataTransfer && Array.from(e.dataTransfer.types).includes('Files')) {
                e.preventDefault();
                container.classList.add('arrastando');
            }
        };
        const aoSair = () => container.classList.remove('arrastando');
        const aoSoltar = (e) => {
            container.classList.remove('arrastando');
            const arquivos = imagensDe(e.dataTransfer && e.dataTransfer.files);
            if (!arquivos.length) return;
            e.preventDefault();
            const ta = container.querySelector('textarea');
            if (ta && e.target === ta && document.caretPositionFromPoint) {
                const p = document.caretPositionFromPoint(e.clientX, e.clientY);
                if (p && p.offsetNode === ta) ta.setSelectionRange(p.offset, p.offset);
            }
            enviar(arquivos);
        };
        const seletor = document.createElement('input');
        seletor.type = 'file';
        seletor.accept = tipos.join(',');
        seletor.multiple = true;
        seletor.style.display = 'none';
        seletor.addEventListener('change', () => { enviar(Array.from(seletor.files)); seletor.value = ''; });
        container.appendChild(seletor);

        container.addEventListener('paste', aoColar);
        container.addEventListener('dragover', aoArrastarSobre);
        container.addEventListener('dragleave', aoSair);
        container.addEventListener('drop', aoSoltar);
        return {
            escolher: () => seletor.click(),
            dispose: () => {
                container.removeEventListener('paste', aoColar);
                container.removeEventListener('dragover', aoArrastarSobre);
                container.removeEventListener('dragleave', aoSair);
                container.removeEventListener('drop', aoSoltar);
                seletor.remove();
            }
        };
    }
};
