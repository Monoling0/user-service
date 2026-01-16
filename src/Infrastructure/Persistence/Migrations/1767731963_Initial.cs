using FluentMigrator;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace Infrastructure.Persistence.Migrations;

#pragma warning disable SA1649
[Migration(1767731963, "Initial")]
public class InitialMigration : IMigration
{
    public void GetUpExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
                           do $$ begin
                               create type role as enum ('admin', 'creator', 'student');
                           exception
                               when duplicate_object then null;
                           end $$;

                           create table if not exists account_passwords
                           (
                               password_id          bigint primary key generated always as identity,
                               password_hash        text not null
                           );

                           create table if not exists accounts
                           (
                               account_id           bigint primary key generated always as identity,

                               role                 role not null,
                               password_id          bigint not null references account_passwords (password_id),
                               email                text unique not null,
                               account_created_at   timestamp with time zone not null,
                               account_updated_at   timestamp with time zone not null
                           );

                           create table if not exists student_profiles
                           (
                               account_id           bigint primary key references accounts (account_id),
                           
                               nickname             text unique not null,
                               profile_photo_url    text
                           );

                           create table if not exists followers
                           (
                               follower_id          bigint references accounts (account_id),
                               followee_id          bigint references accounts (account_id),
                               created_at           timestamp with time zone not null,
                               primary key (follower_id, followee_id)
                           );
                           
                           do $$
                           begin
                               if not exists (select 1 from accounts where email = 'admin@example.com') then
                                   insert into account_passwords (password_hash)
                                   values ('$2a$12$VW5350hCynMdigyR80A2leJbvNSF4A2.QlqLQGIP3UA.kbX.uDNau');
                           
                                   insert into accounts (role, password_id, email, account_created_at, account_updated_at)
                                   values (
                                       'admin',
                                       currval('account_passwords_password_id_seq'),
                                       'admin@example.com',
                                       now(),
                                       now()
                                   );
                           
                                   insert into student_profiles (account_id, nickname)
                                   values (
                                       currval('accounts_account_id_seq'),
                                       'admin'
                                   );
                               end if;
                           end $$;
                           """,
        });
    }

    public void GetDownExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
                           drop table if exists followers;
                           drop table if exists student_profiles;
                           drop table if exists accounts;
                           drop table if exists account_passwords;
                           drop table if exists roles;
                           """,
        });
    }

    public string ConnectionString => throw new NotSupportedException();
}