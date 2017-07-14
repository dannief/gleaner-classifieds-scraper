namespace GleanerClassifieds.Scraper.Messages
{
    public class TryMessage<TMessage>
    {
        public TryMessage(TMessage message, int retries = 0)
        {
            Message = message;
            Retries = retries;
        }

        public TMessage Message { get; private set; }
        public int Retries { get; private set; }
    }
}
