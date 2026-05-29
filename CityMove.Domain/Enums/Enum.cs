namespace CityMove.Domain.Enums;

public enum StatusVeiculo { Ativo, Manutencao, Inativo }
public enum StatusViagem { Planejada, EmAndamento, Concluida, Cancelada }
public enum TipoOcorrencia { Atraso, Acidente, PaneFelec, PaneMecanica, Outro }
public enum StatusOcorrencia { Aberta, EmAnalise, Resolvida }
public enum TipoLinha { Urbana, Intermunicipal, Escolar, Especial }
public enum DiaSemana { Segunda, Terca, Quarta, Quinta, Sexta, Sabado, Domingo }
public enum TipoInfracao { DesvioPadrao, AtrasoRecorrente, VelocidadeExcessiva, Outro }
public enum StatusInfracao { Registrada, Contestada, Confirmada, Arquivada }