import { Link } from 'react-router-dom'

export const HomePage = () => {
  return (
    <div className="page home">
      <section className="hero hero--landing">
        <div>
          <p className="hero__eyebrow">Skaitmeninis brokerių partneris</p>
          <h2>Kurkite, valdykite ir bendrinkite NT skelbimus vienoje erdvėje</h2>
          <p>
            NT Skelbimų sistema leidžia brokeriams rasti savo būsto pirkėjus.
          </p>
          <div className="cta-row">
            <Link className="btn" to="/skelbimai">
              Peržiūrėti skelbimus
            </Link>
            <Link className="btn btn--ghost" to="/apziuros">
              Atvirų durų kalendorius
            </Link>
          </div>
        </div>
        <div className="hero__card">
          <h3>Paprasčiau visiems vaidmenims</h3>
          <ul className="info-list">
            <li>
              <span>Brokeriui</span>
              <strong>Portfelis ir apžiūrų valdymas</strong>
            </li>
            <li>
              <span>Pirkėjui</span>
              <strong>Privačios apžiūros bei kontaktai</strong>
            </li>
            <li>
              <span>Administratoriui</span>
              <strong>Moderavimo įrankiai</strong>
            </li>
          </ul>
        </div>
      </section>
    </div>
  )
}
