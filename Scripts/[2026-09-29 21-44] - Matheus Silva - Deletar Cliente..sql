--select * FROM Clientes
declare @clientid bigint = 22

BEGIN TRAN
--COMMIT
--ROLLBACK

DELETE FROM MovimentacoesEstoque WHERE VendaID in (select VendaID FROM Vendas WHERE ClienteID = @clientid)
DELETE FROM pagamentosvenda WHERE VendaID in (select VendaID FROM Vendas WHERE ClienteID = @clientid)
DELETE FROM ItensVenda WHERE VendaID in (select VendaID FROM Vendas WHERE ClienteID = @clientid)
DELETE FROM AssinaturasEletronicas WHERE VendaID in (select VendaID FROM Vendas WHERE ClienteID = @clientid)
DELETE FROM AssinaturasEletronicas WHERE TermoAberturaID in (select TermoAberturaID FROM TermosAberturaConta WHERE ClienteID = @clientid)
DELETE FROM Clientes WHERE ClienteID = @clientid
DELETE FROM TermosAberturaConta WHERE ClienteID = @clientid
DELETE FROM PessoasAutorizadas WHERE ClienteID = @clientid
DELETE FROM Vendas WHERE ClienteID = @clientid