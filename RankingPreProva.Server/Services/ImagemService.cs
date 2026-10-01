using Microsoft.EntityFrameworkCore;
using RankingPreProva.Server.Data;
using RankingPreProva.Server.Models;
using RankingPreProva.Shared.Dtos;

namespace RankingPreProva.Server.Services;

public class ImagemService(ApplicationDbContext db)
{
    public const int TamanhoMaximo = 1024 * 1024;

    public async Task<ImagemEnviadaDto> SalvarAsync(Stream conteudo, int usuarioId)
    {
        using var ms = new MemoryStream();
        var buffer = new byte[81920];
        int lidos;
        while ((lidos = await conteudo.ReadAsync(buffer)) > 0)
        {
            if (ms.Length + lidos > TamanhoMaximo)
                throw new RegraNegocioException("A imagem deve ter no máximo 1 MB.");
            ms.Write(buffer, 0, lidos);
        }

        var bytes = ms.ToArray();
        if (bytes.Length == 0)
            throw new RegraNegocioException("Imagem vazia.");

        var tipo = DetectarTipo(bytes)
            ?? throw new RegraNegocioException("Formato não suportado. Use PNG, JPG, GIF ou WebP.");

        var imagem = new Imagem
        {
            Id = Guid.NewGuid(),
            AutorId = usuarioId,
            ContentType = tipo,
            Tamanho = bytes.Length,
            Bytes = bytes
        };
        db.Imagens.Add(imagem);
        await db.SaveChangesAsync();

        return new ImagemEnviadaDto { Id = imagem.Id, Url = $"/api/imagens/{imagem.Id}" };
    }

    public Task<Imagem?> ObterAsync(Guid id) =>
        db.Imagens.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);

    private static string? DetectarTipo(ReadOnlySpan<byte> b)
    {
        if (b.Length >= 8 && b[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return "image/png";
        if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
            return "image/jpeg";
        if (b.Length >= 6 && (b[..6].SequenceEqual("GIF87a"u8) || b[..6].SequenceEqual("GIF89a"u8)))
            return "image/gif";
        if (b.Length >= 12 && b[..4].SequenceEqual("RIFF"u8) && b[8..12].SequenceEqual("WEBP"u8))
            return "image/webp";
        return null;
    }
}
