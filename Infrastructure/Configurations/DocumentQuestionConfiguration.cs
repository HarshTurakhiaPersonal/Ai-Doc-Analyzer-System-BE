using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public sealed class DocumentQuestionConfiguration
    : IEntityTypeConfiguration<DocumentQuestion>
{
    public void Configure(
        EntityTypeBuilder<DocumentQuestion> builder)
    {
        builder.ToTable("document_questions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Question)
            .IsRequired();

        builder.Property(x => x.Answer)
            .IsRequired();

        builder.Property(x => x.AskedAt)
            .IsRequired();

        builder.HasIndex(x => x.DocumentId);

        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.Document)
            .WithMany()
            .HasForeignKey(x => x.DocumentId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
    }
}