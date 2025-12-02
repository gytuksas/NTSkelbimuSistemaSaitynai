import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export const HomePage = () => {
  const { user } = useAuth()
  return (
    <div className="page home">
      <section className="hero hero--landing">
        <div>
          <p className="hero__eyebrow">Skaitmeninis brokerių partneris</p>
          <h2>Kurkite, valdykite ir bendrinkite NT skelbimus vienoje erdvėje</h2>
          <p>
            „Atviri namai“ apjungia brokerių portfelį, viešų apžiūrų kalendorių ir pirkėjų tapatybės patvirtinimą.
            Visos funkcijos dabar suskirstytos į aiškius skyrius.
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
              <strong>Profilis ir patvirtinimo užklausos</strong>
            </li>
            <li>
              <span>Administratoriui</span>
              <strong>Moderavimo įrankiai</strong>
            </li>
          </ul>
        </div>
      </section>

      <section className="grid-two">
        <article className="card">
          <h3>Skelbimų galerija</h3>
          <p>
            Vieši lankytojai mato miesto ir kainos informaciją, o prisijungę brokeriai – pilną portfelį su
            nuotraukomis ir vidiniais atributais.
          </p>
          <Link className="btn" to="/skelbimai">
            Atidaryti galeriją
          </Link>
        </article>
        <article className="card">
          <h3>Apžiūrų kalendorius</h3>
          <p>
            Peržiūrėkite viešus atvirų durų laikus arba, jei turite brokerio prieigą, matykite visą suplanuotą
            tvarkaraštį su būsenomis.
          </p>
          <Link className="btn" to="/apziuros">
            Žiūrėti kalendorių
          </Link>
        </article>
      </section>

      <section className="grid-two">
        <article className="card">
          <h3>Brokerių darbo vieta</h3>
          <p>Pastatai, butai, nuotraukos ir skelbimai sujungti į vieną erdvę.</p>
          <Link className="btn btn--ghost" to="/brokeriams">
            Prisijungti kaip brokeris
          </Link>
        </article>
        <article className="card">
          <h3>Mano profilis</h3>
          <p>Stebėkite tapatybės būseną ir siųskite pirkėjo patvirtinimo prašymus.</p>
          <Link className="btn btn--ghost" to={user ? '/profilis' : '/'}>
            {user ? 'Eiti į profilį' : 'Prisijunkite viršuje'}
          </Link>
        </article>
      </section>
    </div>
  )
}
