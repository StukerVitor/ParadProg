namespace ExemploThreadsEExcecoes
{
    class Program
    {
        // ---------- 1) Demonstração de concorrência ----------
        private const int ITERACOES = 1_000_000;
        private static int _contador = 0;
        private static readonly object _lockObj = new object();

        // Incremento sem proteção
        private static void IncrementoSemLock()
        {
            for (int i = 0; i < ITERACOES; i++)
            {
                _contador++;                    // condição de corrida
            }
        }

        // Incremento protegido por lock
        private static void IncrementoComLock()
        {
            for (int i = 0; i < ITERACOES; i++)
            {
                lock (_lockObj)
                {
                    _contador++;                // acesso atômico
                }
            }
        }

        // ---------- 2) Divisão com tratamento de exceção ----------
        private static double Dividir(double numerador, double denominador)
        {
            try
            {
                return numerador / denominador;
            }
            catch (DivideByZeroException)
            {
                Console.WriteLine("Erro: não é possível dividir por zero!");
                return double.NaN;
            }
        }

        // ---------- 3) Verificação de idade ----------
        private static void VerificarIdade(int idade)
        {
            if (idade < 18 || idade > 100)
                throw new IdadeInvalidaException($"Idade {idade} fora do intervalo permitido (18-100).");

            Console.WriteLine($"Idade {idade} é válida.");
        }

        static void Main()
        {
            // ---- Execução SEM lock
            _contador = 0;
            Thread t1 = new Thread(IncrementoSemLock);
            Thread t2 = new Thread(IncrementoSemLock);
            t1.Start(); t2.Start();
            t1.Join(); t2.Join();
            Console.WriteLine($"Sem lock: esperado {ITERACOES * 2:N0}, obtido {_contador:N0}");

            // ---- Execução COM lock
            _contador = 0;
            Thread t3 = new Thread(IncrementoComLock);
            Thread t4 = new Thread(IncrementoComLock);
            t3.Start(); t4.Start();
            t3.Join(); t4.Join();
            Console.WriteLine($"Com lock: esperado {ITERACOES * 2:N0}, obtido {_contador:N0}");

            Console.WriteLine("------------------------------------------------");

            // ---- Teste da divisão
            Console.WriteLine($"10 / 2 = {Dividir(10, 2)}");
            Console.WriteLine($"10 / 0 = {Dividir(10, 0)}");

            Console.WriteLine("------------------------------------------------");

            // ---- Teste da idade
            try
            {
                VerificarIdade(25);  // ok
                VerificarIdade(10);  // lança exceção
            }
            catch (IdadeInvalidaException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    // ---------- 3) Exceção personalizada ----------
    public class IdadeInvalidaException : Exception
    {
        public IdadeInvalidaException() { }
        public IdadeInvalidaException(string message) : base(message) { }
        public IdadeInvalidaException(string message, Exception inner) : base(message, inner) { }
    }
}

/* Resultados
    Sem lock: esperado 2.000.000, obtido 1.111.318
    Com lock: esperado 2.000.000, obtido 2.000.000

    Sem a proteção de lock, as duas threads acessaram a variável contadora 
    simultaneamente, fazendo com que diversos incrementos se perdessem devido 
    às condições de corrida; por isso o total final (1 111 318) ficou bem 
    abaixo do esperado (2 000 000). Já com o lock, cada incremento foi 
    executado de forma mutuamente exclusiva, eliminando interferências entre 
    as threads e garantindo o valor exato de 2 000 000. Esses resultados 
    demonstram na prática como a sincronização é essencial para preservar a 
    consistência de dados em ambientes concorrentes.
    ------------------------------------------------
    10 / 2 = 5
    10 / 0 = 8
    ------------------------------------------------
    Idade 25 é válida.
    Idade 10 fora do intervalo permitido (18-100).
*/
