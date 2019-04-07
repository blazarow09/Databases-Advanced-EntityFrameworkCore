using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P03_FootballBetting.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P03_FootballBetting.Data
{
    public class FootballBettingContext : DbContext
    {
        public DbSet<Team> Teams { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Town> Towns { get; set; }
        public DbSet<Bet> Bets { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerStatistic> PlayerStatistics { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Config.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureTeamEntity(modelBuilder);

            ConfigureColorEntity(modelBuilder);

            ConfigureTownEntity(modelBuilder);

            ConfigureGameEntity(modelBuilder);

            ConfigureCountryEntity(modelBuilder);

            ConfigurePlayerEntity(modelBuilder);

            ConfigurePositionEntity(modelBuilder);

            ConfigurePlayerStatisticEntity(modelBuilder);

            ConfigureBetEntity(modelBuilder);

            ConfigureUserEntity(modelBuilder);
        }

        private void ConfigureUserEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .HasKey(x => x.UserId);

            modelBuilder
                .Entity<User>()
                .Property(e => e.Username)
                .IsUnicode(true)
                .IsRequired(true);

            modelBuilder
                .Entity<User>()
                .Property(e => e.Password)
                .IsUnicode(false)
                .IsRequired(true);

            modelBuilder
                .Entity<User>()
                .Property(e => e.Email)
                .IsUnicode(false)
                .IsRequired(true);

            modelBuilder
                .Entity<User>()
                .Property(e => e.Name)
                .IsUnicode(true)
                .IsRequired(true);
        }

        private void ConfigureBetEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Bet>()
                .HasKey(x => x.BetId);

            modelBuilder
                .Entity<Bet>()
                .HasOne(x => x.Game)
                .WithMany(x => x.Bets)
                .HasForeignKey(x => x.GameId);

            modelBuilder
                .Entity<Bet>()
                .HasOne(x => x.User)
                .WithMany(x => x.Bets)
                .HasForeignKey(x => x.UserId);

            modelBuilder
                .Entity<Bet>()
                .Property(x => x.Prediction)
                .IsRequired(true);

            modelBuilder
                .Entity<Bet>()
                .Property(x => x.DateTime)
                .HasDefaultValue(DateTime.Now);

            modelBuilder
                .Entity<Bet>()
                .Property(x => x.UserId)
                .IsRequired(true);

            modelBuilder
                .Entity<Bet>()
                .Property(x => x.GameId)
                .IsRequired(true);
        }

        private void ConfigurePlayerStatisticEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<PlayerStatistic>()
                .HasKey(x => new { x.GameId, x.PlayerId });

            modelBuilder
               .Entity<PlayerStatistic>()
               .HasOne(x => x.Game)
               .WithMany(x => x.PlayerStatistics)
               .HasForeignKey(x => x.GameId);

            modelBuilder
                .Entity<PlayerStatistic>()
                .HasOne(x => x.Player)
                .WithMany(x => x.PlayerStatistics)
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigurePositionEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Position>()
                .HasKey(x => x.PositionId);

            modelBuilder
                .Entity<Position>()
                .HasAlternateKey(x => x.Name);

            modelBuilder
                .Entity<Position>()
                .Property(x => x.Name)
                .IsUnicode(true)
                .IsRequired(true);
        }

        private void ConfigurePlayerEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Player>()
                .HasKey(x => x.PlayerId);

            modelBuilder
                .Entity<Player>()
                .HasOne(x => x.Team)
                .WithMany(x => x.Players)
                .HasForeignKey(x => x.TeamId);

            modelBuilder
               .Entity<Player>()
               .HasOne(x => x.Position)
               .WithMany(x => x.Players)
               .HasForeignKey(x => x.PositionId);

            modelBuilder
                .Entity<Player>()
                .Property(x => x.Name)
                .IsRequired(true);

            modelBuilder
            .Entity<Player>()
            .Property(x => x.IsInjured)
            .IsRequired(true)
            .HasDefaultValue(false);
        }

        private void ConfigureCountryEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Country>()
                .HasKey(x => x.CountryId);

            modelBuilder
                .Entity<Country>()
                .HasAlternateKey(x => x.Name);

            modelBuilder
                .Entity<Country>()
                .Property(x => x.Name)
                .IsUnicode(true)
                .IsRequired(true);
        }

        private void ConfigureTownEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Town>()
                .HasKey(x => x.TownId);

            modelBuilder
                .Entity<Town>()
                .HasOne(x => x.Country)
                .WithMany(x => x.Towns)
                .HasForeignKey(x => x.CountryId);

            modelBuilder
                .Entity<Town>()
                .HasAlternateKey(x => new { x.Name, x.CountryId });

            modelBuilder
                .Entity<Town>()
                .Property(x => x.Name)
                .IsRequired(true);
        }

        private void ConfigureGameEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Game>()
                .HasKey(x => x.GameId);

            modelBuilder
                .Entity<Game>()
                .HasOne(x => x.HomeTeam)
                .WithMany(x => x.HomeGames)
                .HasForeignKey(x => x.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Game>()
                .HasOne(x => x.AwayTeam)
                .WithMany(x => x.AwayGames)
                .HasForeignKey(x => x.AwayTeamId);
        }

        private void ConfigureColorEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Color>()
                .HasKey(c => c.ColorId);

            modelBuilder
                .Entity<Color>()
                .HasAlternateKey(x => x.Name);

            modelBuilder
                .Entity<Color>()
                .Property(x => x.Name)
                .IsUnicode(true)
                .IsRequired(true);
        }

        private void ConfigureTeamEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Team>()
                .HasKey(t => t.TeamId);

            modelBuilder
                .Entity<Team>()
                .HasOne(x => x.PrimaryKitColor)
                .WithMany(x => x.PrimaryKitTeams)
                .HasForeignKey(x => x.PrimaryKitColorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<Team>()
                .HasOne(x => x.SecondaryKitColor)
                .WithMany(x => x.SecondaryKitTeams)
                .HasForeignKey(x => x.SecondaryKitColorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<Team>()
                .HasOne(x => x.Town)
                .WithMany(x => x.Teams)
                .HasForeignKey(x => x.TownId);

            modelBuilder
                .Entity<Team>()
                .Property(x => x.Name)
                .IsRequired(true);
            
            modelBuilder
                .Entity<Team>()
                .Property(x => x.LogoUrl)
                .IsUnicode(false)
                .IsRequired(true);

            modelBuilder
                .Entity<Team>()
                .Property(x => x.Initials)
                .HasColumnType("NCHAR(3)")
                .IsRequired(true);
        }
    }
}
